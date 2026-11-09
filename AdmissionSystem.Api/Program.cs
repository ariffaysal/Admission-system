using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using AdmissionSystem.Api.Data;
using AdmissionSystem.Api.Interfaces;
using AdmissionSystem.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Database Configuration
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                      ?? "Host=localhost;Database=admission_db;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AdmissionDbContext>(options =>
    options.UseNpgsql(connectionString));

// Dependency Injection
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IAdmissionService, AdmissionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Set port to 8080
app.Run("http://0.0.0.0:8080");
