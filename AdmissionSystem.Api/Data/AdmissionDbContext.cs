using Microsoft.EntityFrameworkCore;
using AdmissionSystem.Api.Entities;

namespace AdmissionSystem.Api.Data
{
    public class AdmissionDbContext : DbContext
    {
        public AdmissionDbContext(DbContextOptions<AdmissionDbContext> options) : base(options) { }

        public DbSet<ClassOffering> ClassOfferings { get; set; } = null!;
        public DbSet<AdmissionHold> AdmissionHolds { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ClassOffering configuration
            modelBuilder.Entity<ClassOffering>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClassName).IsRequired().HasMaxLength(255);

                // Constraint: AvailableSeats cannot be negative
                entity.ToTable(t => t.HasCheckConstraint("CK_ClassOffering_AvailableSeats_NonNegative", "available_seats >= 0"));

                // Constraint: AvailableSeats cannot exceed TotalSeats
                entity.ToTable(t => t.HasCheckConstraint("CK_ClassOffering_SeatLimit", "available_seats <= total_seats"));
            });

            // AdmissionHold configuration
            modelBuilder.Entity<AdmissionHold>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StudentId).IsRequired().HasMaxLength(100);

                entity.HasOne(h => h.ClassOffering)
                      .WithMany(c => c.Holds)
                      .HasForeignKey(h => h.ClassOfferingId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Partial Unique Index: A student can only have one active (unconfirmed) hold per class
                entity.HasIndex(h => new { h.StudentId, h.ClassOfferingId })
                      .IsUnique()
                      .HasFilter("\"IsConfirmed\" = false");

                // Indices for performance
                entity.HasIndex(h => h.StudentId);
                entity.HasIndex(h => h.ClassOfferingId);
                entity.HasIndex(h => h.ExpiresAt);
            });
        }
    }
}
