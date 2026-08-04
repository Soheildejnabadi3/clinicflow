using Microsoft.EntityFrameworkCore;
using MediatR;
using ClinicFlow.Api.Common;
using ClinicFlow.Api.Data;
using ClinicFlow.Api.DTOs.Appointment;
using ClinicFlow.Api.Events;
using ClinicFlow.Api.Exceptions;
using ClinicFlow.Api.Models;
using ClinicFlow.Api.Services.Interfaces;

namespace ClinicFlow.Api.Services;

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _context;
    private readonly IProviderService _providerService;
    private readonly IPatientService _patientService;
    private readonly IMediator _mediator;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        AppDbContext context,
        IProviderService providerService,
        IPatientService patientService,
        IMediator mediator,
        ILogger<AppointmentService> logger)
    {
        _context = context;
        _providerService = providerService;
        _patientService = patientService;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<List<AppointmentDto>> GetAllAsync(
        DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(a => a.StartTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.StartTime <= toDate.Value);

        var appointments = await query
            .OrderBy(a => a.StartTime)
            .ToListAsync();

        return appointments.Select(MapToDto).ToList();
    }

    public async Task<AppointmentDto?> GetByIdAsync(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .FirstOrDefaultAsync(a => a.Id == id);

        return appointment == null ? null : MapToDto(appointment);
    }

    public async Task<List<AppointmentDto>> GetByPatientIdAsync(int patientId)
    {
        var appointments = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.StartTime)
            .ToListAsync();

        return appointments.Select(MapToDto).ToList();
    }

    public async Task<List<AppointmentDto>> GetByProviderIdAsync(
        int providerId, DateTime? date = null)
    {
        var query = _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Where(a => a.ProviderId == providerId);

        if (date.HasValue)
        {
            var dayStart = date.Value.Date;
            var dayEnd = dayStart.AddDays(1);
            query = query.Where(a => a.StartTime >= dayStart && a.StartTime < dayEnd);
        }

        var appointments = await query
            .OrderBy(a => a.StartTime)
            .ToListAsync();

        return appointments.Select(MapToDto).ToList();
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto)
    {
        // Pin request times to UTC before validating against DateTime.UtcNow.
        dto.StartTime = dto.StartTime.ToUtc();
        dto.EndTime = dto.EndTime.ToUtc();

        // Validate patient exists
        if (!await _patientService.ExistsAsync(dto.PatientId))
            throw new NotFoundException("Patient", dto.PatientId);

        // Validate provider exists
        if (!await _providerService.ExistsAsync(dto.ProviderId))
            throw new NotFoundException("Provider", dto.ProviderId);

        // Validate time range
        var errors = new List<string>();

        if (dto.StartTime >= dto.EndTime)
            errors.Add("Start time must be before end time.");

        if (dto.StartTime < DateTime.UtcNow)
            errors.Add("Cannot book appointments in the past.");

        var duration = dto.EndTime - dto.StartTime;
        if (duration.TotalMinutes < 15)
            errors.Add("Appointment must be at least 15 minutes.");
        if (duration.TotalHours > 4)
            errors.Add("Appointment cannot exceed 4 hours.");

        if (errors.Count > 0)
            throw new Exceptions.ValidationException(errors);

        // Check provider availability
        var isAvailable = await _providerService.IsAvailableAsync(
            dto.ProviderId, dto.StartTime, dto.EndTime);

        if (!isAvailable)
            throw new ConflictException(
                "Provider is not available during the requested time slot. " +
                "Either outside working hours or another appointment exists.");

        // Check patient doesn't have overlapping appointment
        var patientConflict = await _context.Appointments.AnyAsync(a =>
            a.PatientId == dto.PatientId &&
            a.Status != AppointmentStatus.Cancelled &&
            a.StartTime < dto.EndTime &&
            a.EndTime > dto.StartTime);

        if (patientConflict)
            throw new ConflictException("Patient already has an appointment during this time.");

        // Create appointment
        var appointment = new Appointment
        {
            PatientId = dto.PatientId,
            ProviderId = dto.ProviderId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = AppointmentStatus.Scheduled,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Created appointment {Id} for patient {PatientId} with provider {ProviderId} at {Time}",
            appointment.Id, appointment.PatientId, appointment.ProviderId, appointment.StartTime);

        // Publish domain event (event-driven architecture)
        await _mediator.Publish(new AppointmentCreatedEvent(
            appointment.Id,
            appointment.PatientId,
            appointment.ProviderId,
            appointment.StartTime));

        // Reload with navigation properties for DTO mapping
        await _context.Entry(appointment).Reference(a => a.Patient).LoadAsync();
        await _context.Entry(appointment).Reference(a => a.Provider).LoadAsync();

        return MapToDto(appointment);
    }

    public async Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto dto)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null) return null;

        dto.StartTime = dto.StartTime.ToUtc();
        dto.EndTime = dto.EndTime.ToUtc();

        // Cannot modify cancelled or completed appointments
        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new Exceptions.ValidationException("Cannot modify a cancelled appointment.");
        if (appointment.Status == AppointmentStatus.Completed)
            throw new Exceptions.ValidationException("Cannot modify a completed appointment.");

        // Update time if provided
        if (dto.StartTime.HasValue && dto.EndTime.HasValue)
        {
            if (dto.StartTime.Value >= dto.EndTime.Value)
                throw new Exceptions.ValidationException("Start time must be before end time.");

            // The appointment being moved must not count as a conflict with itself.
            var isAvailable = await _providerService.IsAvailableAsync(
                appointment.ProviderId, dto.StartTime.Value, dto.EndTime.Value,
                excludeAppointmentId: id);

            if (!isAvailable)
                throw new ConflictException("Provider is not available during the new time.");

            appointment.StartTime = dto.StartTime.Value;
            appointment.EndTime = dto.EndTime.Value;
        }

        if (dto.Status != null && Enum.TryParse<AppointmentStatus>(dto.Status, out var status))
        {
            appointment.Status = status;
        }

        if (dto.Notes != null)
        {
            appointment.Notes = dto.Notes;
        }

        await _context.SaveChangesAsync();
        return MapToDto(appointment);
    }

    public async Task<AppointmentDto?> CancelAsync(int id, string reason)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null) return null;

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new Exceptions.ValidationException("Appointment is already cancelled.");

        if (appointment.Status == AppointmentStatus.Completed)
            throw new Exceptions.ValidationException("Cannot cancel a completed appointment.");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.Notes = string.IsNullOrWhiteSpace(appointment.Notes)
            ? $"Cancelled: {reason}"
            : $"{appointment.Notes}\nCancelled: {reason}";

        await _context.SaveChangesAsync();

        // Publish cancellation event
        await _mediator.Publish(new AppointmentCancelledEvent(
            appointment.Id,
            appointment.PatientId,
            appointment.ProviderId,
            appointment.StartTime,
            reason));

        _logger.LogInformation("Cancelled appointment {Id}. Reason: {Reason}", id, reason);

        return MapToDto(appointment);
    }

    public async Task<AppointmentDto?> CompleteAsync(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null) return null;

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new Exceptions.ValidationException("Cannot complete a cancelled appointment.");

        appointment.Status = AppointmentStatus.Completed;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Completed appointment {Id}", id);

        return MapToDto(appointment);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return false;

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var weekEnd = weekStart.AddDays(7);

        // Materialise first: MapToDto is a C# method and cannot be translated to SQL,
        // so projecting with it inside the query would throw at runtime.
        var nextFive = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Where(a => a.StartTime > DateTime.UtcNow &&
                        a.Status == AppointmentStatus.Scheduled)
            .OrderBy(a => a.StartTime)
            .Take(5)
            .ToListAsync();

        return new DashboardStatsDto
        {
            TotalPatients = await _context.Patients.CountAsync(),
            TotalProviders = await _context.Providers.CountAsync(),
            TodayAppointments = await _context.Appointments
                .CountAsync(a => a.StartTime >= today && a.StartTime < tomorrow &&
                                 a.Status != AppointmentStatus.Cancelled),
            UpcomingAppointments = await _context.Appointments
                .CountAsync(a => a.StartTime > DateTime.UtcNow &&
                                 a.Status == AppointmentStatus.Scheduled),
            CancelledThisWeek = await _context.Appointments
                .CountAsync(a => a.StartTime >= weekStart &&
                                 a.StartTime < weekEnd &&
                                 a.Status == AppointmentStatus.Cancelled),
            CompletedThisWeek = await _context.Appointments
                .CountAsync(a => a.StartTime >= weekStart &&
                                 a.StartTime < weekEnd &&
                                 a.Status == AppointmentStatus.Completed),
            NextFiveAppointments = nextFive.Select(MapToDto).ToList()
        };
    }

    private static AppointmentDto MapToDto(Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = appointment.Patient != null
                ? $"{appointment.Patient.FirstName} {appointment.Patient.LastName}"
                : string.Empty,
            ProviderId = appointment.ProviderId,
            ProviderName = appointment.Provider != null
                ? $"Dr. {appointment.Provider.FirstName} {appointment.Provider.LastName}"
                : string.Empty,
            ProviderSpecialty = appointment.Provider?.Specialty ?? string.Empty,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status.ToString(),
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt
        };
    }
}