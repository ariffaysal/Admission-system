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
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_ClassOffering_TotalSeats_Positive", "\"TotalSeats\" > 0");
                    t.HasCheckConstraint("CK_ClassOffering_AvailableSeats_NonNegative", "\"AvailableSeats\" >= 0");
                    t.HasCheckConstraint("CK_ClassOffering_SeatLimit", "\"AvailableSeats\" <= \"TotalSeats\"");
                });
            });

            // AdmissionHold configuration
            modelBuilder.Entity<AdmissionHold>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StudentId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("HELD");

                entity.HasOne(h => h.ClassOffering)
                      .WithMany(c => c.Holds)
                      .HasForeignKey(h => h.ClassOfferingId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Partial Unique Index: A student can only have one active (unconfirmed) hold per class
                entity.HasIndex(h => new { h.StudentId, h.ClassOfferingId })
                      .IsUnique()
                      .HasFilter("\"Status\" IN ('HELD', 'CONFIRMED')");

                // Indices for performance
                entity.HasIndex(h => h.StudentId);
                entity.HasIndex(h => h.ClassOfferingId);
                entity.HasIndex(h => h.ExpiresAt);
            });
        }
    }
}
