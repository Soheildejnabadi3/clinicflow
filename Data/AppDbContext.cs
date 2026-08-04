using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ClinicFlow.Api.Models;

namespace ClinicFlow.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships (like Prisma schema)
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasOne(a => a.Patient)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(a => a.PatientId);

            entity.HasOne(a => a.Provider)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(a => a.ProviderId);

            // Index for common queries
            entity.HasIndex(a => a.StartTime);
            entity.HasIndex(a => new { a.ProviderId, a.StartTime });
        });

        modelBuilder.Entity<AvailabilitySlot>(entity =>
        {
            entity.HasOne(s => s.Provider)
                  .WithMany(p => p.AvailabilitySlots)
                  .HasForeignKey(s => s.ProviderId);
        });

        ApplyUtcDateTimeConversion(modelBuilder);
    }

    // Npgsql maps DateTime to "timestamp with time zone" and refuses to write any
    // value whose Kind is not Utc. JSON request bodies deserialise to Unspecified
    // (or Local, when an offset is supplied), so normalise every DateTime here
    // rather than at each call site.
    private static void ApplyUtcDateTimeConversion(ModelBuilder modelBuilder)
    {
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc
                ? v
                : v.Kind == DateTimeKind.Local
                    ? v.ToUniversalTime()
                    : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableUtcConverter = new ValueConverter<DateTime?, DateTime?>(
            v => !v.HasValue
                ? v
                : v.Value.Kind == DateTimeKind.Utc
                    ? v
                    : v.Value.Kind == DateTimeKind.Local
                        ? v.Value.ToUniversalTime()
                        : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc),
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(utcConverter);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(nullableUtcConverter);
            }
        }
    }
}