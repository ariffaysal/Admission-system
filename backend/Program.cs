using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connection = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("Admission")
    ?? "Host=localhost;Port=5432;Database=school_admission;Username=postgres;Password=postgres";
builder.Services.AddDbContext<AdmissionDb>(o => o.UseNpgsql(connection));
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins("http://localhost:5173", "http://localhost:3000").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddHostedService<ExpiryWorker>();
var app = builder.Build();
app.UseCors();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/classes", async (AdmissionDb db) => Results.Ok(await db.Classes.Select(c => new ClassView(c.Id, c.Name, c.TotalSeats, c.HeldSeats, c.ConfirmedSeats, c.TotalSeats-c.HeldSeats-c.ConfirmedSeats)).ToListAsync()));
app.MapGet("/classes/{id:guid}", async (Guid id, AdmissionDb db) => { var c = await db.Classes.FindAsync(id); return c is null ? Results.NotFound() : Results.Ok(View(c)); });
app.MapPost("/classes", async (CreateClass req, AdmissionDb db) => {
    if (string.IsNullOrWhiteSpace(req.Name) || req.Name.Length > 150 || req.TotalSeats <= 0) return Results.BadRequest(new { error = "Provide a class name and positive seat capacity." });
    var c = new ClassOffering { Name = req.Name.Trim(), TotalSeats = req.TotalSeats }; db.Classes.Add(c); await db.SaveChangesAsync(); return Results.Created($"/classes/{c.Id}", View(c));
});
app.MapPost("/classes/{id:guid}/holds", async (Guid id, HttpRequest request, AdmissionDb db) => {
    var student = Student(request); if (student is null) return Results.BadRequest(new { error = "Valid X-Student-Id header is required (5–50 digits)." });
    await using var tx = await db.Database.BeginTransactionAsync();
    var c = await db.Classes.FromSqlInterpolated($"SELECT * FROM \"Classes\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync();
    if (c is null) return Results.NotFound(new { error = "Class not found." });
    await Expire(db, c.Id);
    if (await db.Holds.AnyAsync(h => h.ClassId == id && h.StudentId == student && (h.Status == "HELD" || h.Status == "CONFIRMED"))) return Results.Conflict(new { error = "An active application already exists for this student and class." });
    if (c.HeldSeats + c.ConfirmedSeats >= c.TotalSeats) return Results.Conflict(new { error = "No seats are available." });
    var hold = new AdmissionHold { ClassId = id, StudentId = student, ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Clamp(int.TryParse(Environment.GetEnvironmentVariable("HOLD_DURATION_SECONDS"), out var seconds) ? seconds : 600, 1, 86400)) };
    db.Holds.Add(hold); c.HeldSeats++; await db.SaveChangesAsync(); await tx.CommitAsync();
    return Results.Created($"/holds/{hold.Id}", new { holdId=hold.Id, classId=id, studentId=student, status=hold.Status, expiresAt=hold.ExpiresAt });
});
app.MapGet("/holds/{id:guid}", async (Guid id, HttpRequest request, AdmissionDb db) => {
    var student=Student(request); if(student is null) return Results.BadRequest(new {error="Valid X-Student-Id header is required."});
    var h=await db.Holds.Include(x=>x.Class).SingleOrDefaultAsync(x=>x.Id==id && x.StudentId==student); if(h is null) return Results.NotFound();
    await ExpireOne(db,h); return Results.Ok(new {holdId=h.Id,classId=h.ClassId,className=h.Class.Name,status=h.Status,expiresAt=h.ExpiresAt});
});
app.MapGet("/me/holds", async (HttpRequest request, AdmissionDb db) => {
    var student=Student(request); if(student is null) return Results.BadRequest(new {error="Valid X-Student-Id header is required."});
    var holds=await db.Holds.Include(h=>h.Class).Where(h=>h.StudentId==student).OrderByDescending(h=>h.CreatedAt).ToListAsync(); foreach(var h in holds) await ExpireOne(db,h);
    return Results.Ok(holds.Select(h=>new {holdId=h.Id,classId=h.ClassId,className=h.Class.Name,status=h.Status,expiresAt=h.ExpiresAt}));
});
app.MapPost("/holds/{id:guid}/confirm", async (Guid id, HttpRequest request, AdmissionDb db) => {
    var student=Student(request); if(student is null) return Results.BadRequest(new {error="Valid X-Student-Id header is required."});
    await using var tx=await db.Database.BeginTransactionAsync(); var h=await db.Holds.SingleOrDefaultAsync(x=>x.Id==id); if(h is null) return Results.NotFound();
    var c=await db.Classes.FromSqlInterpolated($"SELECT * FROM \"Classes\" WHERE \"Id\" = {h.ClassId} FOR UPDATE").SingleAsync(); h=await db.Holds.SingleAsync(x=>x.Id==id);
    if(h.StudentId!=student) return Results.StatusCode(403); if(h.Status=="CONFIRMED") return Results.Ok(new {holdId=h.Id,status=h.Status});
    if(h.Status!="HELD" || h.ExpiresAt<=DateTimeOffset.UtcNow) { if(h.Status=="HELD"){h.Status="EXPIRED";c.HeldSeats--;} await db.SaveChangesAsync(); await tx.CommitAsync(); return Results.Conflict(new {error="Reservation is no longer active."}); }
    h.Status="CONFIRMED"; c.HeldSeats--; c.ConfirmedSeats++; await db.SaveChangesAsync(); await tx.CommitAsync(); return Results.Ok(new {holdId=h.Id,status=h.Status});
});
app.MapDelete("/holds/{id:guid}", async (Guid id,HttpRequest request,AdmissionDb db)=>{
    var student=Student(request); if(student is null) return Results.BadRequest(new {error="Valid X-Student-Id header is required."});
    await using var tx=await db.Database.BeginTransactionAsync(); var h=await db.Holds.SingleOrDefaultAsync(x=>x.Id==id); if(h is null)return Results.NotFound();
    var c=await db.Classes.FromSqlInterpolated($"SELECT * FROM \"Classes\" WHERE \"Id\" = {h.ClassId} FOR UPDATE").SingleAsync(); h=await db.Holds.SingleAsync(x=>x.Id==id);
    if(h.StudentId!=student)return Results.StatusCode(403); if(h.Status=="HELD"){h.Status="CANCELLED";c.HeldSeats--;} await db.SaveChangesAsync(); await tx.CommitAsync(); return Results.NoContent();
});
app.Run();

static string? Student(HttpRequest r) { var s=r.Headers["X-Student-Id"].FirstOrDefault(); return s is not null && s.Length is >=5 and <=50 && s.All(char.IsDigit) ? s : null; }
static ClassView View(ClassOffering c)=>new(c.Id,c.Name,c.TotalSeats,c.HeldSeats,c.ConfirmedSeats,c.TotalSeats-c.HeldSeats-c.ConfirmedSeats);
static async Task Expire(AdmissionDb db,Guid classId){var expired=await db.Holds.Where(h=>h.ClassId==classId&&h.Status=="HELD"&&h.ExpiresAt<=DateTimeOffset.UtcNow).ToListAsync(); if(expired.Count==0)return; var c=await db.Classes.FindAsync(classId); foreach(var h in expired){h.Status="EXPIRED";if(c is not null)c.HeldSeats--;} await db.SaveChangesAsync();}
static async Task ExpireOne(AdmissionDb db,AdmissionHold h){if(h.Status=="HELD"&&h.ExpiresAt<=DateTimeOffset.UtcNow){h.Status="EXPIRED";h.Class.HeldSeats--;await db.SaveChangesAsync();}}
record CreateClass(string Name,int TotalSeats);
record ClassView(Guid Id,string Name,int TotalSeats,int HeldSeats,int ConfirmedSeats,int AvailableSeats);
class ClassOffering { public Guid Id {get;set;}=Guid.NewGuid(); public string Name {get;set;}=""; public int TotalSeats {get;set;} public int HeldSeats {get;set;} public int ConfirmedSeats {get;set;} }
class AdmissionHold { public Guid Id {get;set;}=Guid.NewGuid(); public Guid ClassId {get;set;} public ClassOffering Class {get;set;}=null!; public string StudentId {get;set;}=""; public string Status {get;set;}="HELD"; public DateTimeOffset ExpiresAt {get;set;} public DateTimeOffset CreatedAt {get;set;}=DateTimeOffset.UtcNow; }
class AdmissionDb(DbContextOptions<AdmissionDb> options):DbContext(options){public DbSet<ClassOffering> Classes=>Set<ClassOffering>(); public DbSet<AdmissionHold> Holds=>Set<AdmissionHold>(); protected override void OnModelCreating(ModelBuilder m){m.Entity<ClassOffering>().ToTable(t=>t.HasCheckConstraint("CK_Class_Capacity","\"TotalSeats\" > 0 AND \"HeldSeats\" >= 0 AND \"ConfirmedSeats\" >= 0 AND \"HeldSeats\" + \"ConfirmedSeats\" <= \"TotalSeats\"")); m.Entity<AdmissionHold>().HasIndex(h=>new{h.StudentId,h.ClassId}).IsUnique().HasFilter("\"Status\" IN ('HELD','CONFIRMED')");}}
class ExpiryWorker(IServiceScopeFactory scopes):BackgroundService { protected override async Task ExecuteAsync(CancellationToken stoppingToken){while(!stoppingToken.IsCancellationRequested){try{using var scope=scopes.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AdmissionDb>(); await db.Database.EnsureCreatedAsync(stoppingToken); var expired=await db.Holds.Include(h=>h.Class).Where(h=>h.Status=="HELD"&&h.ExpiresAt<=DateTimeOffset.UtcNow).ToListAsync(stoppingToken); foreach(var h in expired){h.Status="EXPIRED";h.Class.HeldSeats--;} if(expired.Count>0)await db.SaveChangesAsync(stoppingToken);}catch{} await Task.Delay(TimeSpan.FromSeconds(5),stoppingToken);}}}
