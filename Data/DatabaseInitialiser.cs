using Microsoft.EntityFrameworkCore;
using ClinicFlow.Api.Models;

namespace ClinicFlow.Api.Data;

public static class DatabaseInitialiser
{
    /// <summary>
    /// Applies pending migrations and, on an empty database, inserts a small set of
    /// demo providers, availability slots and patients so the API and UI have
    /// something to show on first run.
    /// </summary>
    public static async Task InitialiseDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DatabaseInitialiser));

        await context.Database.MigrateAsync();

        if (await context.Providers.AnyAsync())
        {
            logger.LogInformation("Database already seeded; skipping demo data.");
            return;
        }

        var providers = new List<Provider>
        {
            new() { FirstName = "Ada",   LastName = "Okonkwo", Specialty = "Cardiology",   Email = "ada.okonkwo@clinicflow.test" },
            new() { FirstName = "Ravi",  LastName = "Menon",   Specialty = "Dermatology",  Email = "ravi.menon@clinicflow.test" },
            new() { FirstName = "Lena",  LastName = "Fischer", Specialty = "Paediatrics",  Email = "lena.fischer@clinicflow.test" }
        };
        context.Providers.AddRange(providers);
        await context.SaveChangesAsync();

        // Weekday availability, 09:00-17:00, for every seeded provider.
        var weekdays = new[]
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday
        };

        context.AvailabilitySlots.AddRange(
            from provider in providers
            from day in weekdays
            select new AvailabilitySlot
            {
                ProviderId = provider.Id,
                DayOfWeek = day,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                IsActive = true
            });

        var patients = new List<Patient>
        {
            new() { FirstName = "Jonah",  LastName = "Alvarez", Email = "jonah.alvarez@example.test",  Phone = "555-0101", DateOfBirth = new DateTime(1984, 3, 12, 0, 0, 0, DateTimeKind.Utc) },
            new() { FirstName = "Priya",  LastName = "Raghavan", Email = "priya.raghavan@example.test", Phone = "555-0102", DateOfBirth = new DateTime(1992, 11, 2, 0, 0, 0, DateTimeKind.Utc) },
            new() { FirstName = "Tomas",  LastName = "Brandt",  Email = "tomas.brandt@example.test",   Phone = "555-0103", DateOfBirth = new DateTime(1970, 7, 25, 0, 0, 0, DateTimeKind.Utc) },
            new() { FirstName = "Mei",    LastName = "Chen",    Email = "mei.chen@example.test",       Phone = "555-0104", DateOfBirth = new DateTime(2015, 1, 9, 0, 0, 0, DateTimeKind.Utc) }
        };
        context.Patients.AddRange(patients);
        await context.SaveChangesAsync();

        // A couple of upcoming appointments on the next weekday, inside working hours.
        var nextWeekday = DateTime.UtcNow.Date.AddDays(1);
        while (nextWeekday.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            nextWeekday = nextWeekday.AddDays(1);

        context.Appointments.AddRange(
            new Appointment
            {
                PatientId = patients[0].Id,
                ProviderId = providers[0].Id,
                StartTime = nextWeekday.AddHours(10),
                EndTime = nextWeekday.AddHours(10).AddMinutes(30),
                Status = AppointmentStatus.Scheduled,
                Notes = "Annual cardiac review"
            },
            new Appointment
            {
                PatientId = patients[1].Id,
                ProviderId = providers[1].Id,
                StartTime = nextWeekday.AddHours(11),
                EndTime = nextWeekday.AddHours(11).AddMinutes(45),
                Status = AppointmentStatus.Confirmed,
                Notes = "Follow-up on skin biopsy"
            });

        await context.SaveChangesAsync();

        logger.LogInformation(
            "Seeded {Providers} providers, {Patients} patients and 2 appointments.",
            providers.Count, patients.Count);
    }
}
