using Microsoft.EntityFrameworkCore;
using ClinicFlow.Api.Common;
using ClinicFlow.Api.Data;
using ClinicFlow.Api.DTOs.Patient;
using ClinicFlow.Api.Exceptions;
using ClinicFlow.Api.Models;
using ClinicFlow.Api.Services.Interfaces;

namespace ClinicFlow.Api.Services;

public class PatientService : IPatientService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PatientService> _logger;

    public PatientService(AppDbContext context, ILogger<PatientService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<PatientDto>> GetAllAsync(string? searchTerm = null)
    {
        var query = _context.Patients
            .Include(p => p.Appointments)
            .AsQueryable();

        // Search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                p.Email.ToLower().Contains(term));
        }

        var patients = await query
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();

        return patients.Select(MapToDto).ToList();
    }

    public async Task<PatientDto?> GetByIdAsync(int id)
    {
        var patient = await _context.Patients
            .Include(p => p.Appointments)
            .FirstOrDefaultAsync(p => p.Id == id);

        return patient == null ? null : MapToDto(patient);
    }

    public async Task<PatientDto> CreateAsync(CreatePatientDto dto)
    {
        dto.DateOfBirth = dto.DateOfBirth.ToUtc();

        // Check for duplicate email
        var existingPatient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Email.ToLower() == dto.Email.ToLower());

        if (existingPatient != null)
        {
            throw new ConflictException($"A patient with email '{dto.Email}' already exists.");
        }

        // Validate date of birth
        if (dto.DateOfBirth >= DateTime.UtcNow)
        {
            throw new Exceptions.ValidationException("Date of birth must be in the past.");
        }

        var patient = new Patient
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            Phone = dto.Phone.Trim(),
            DateOfBirth = dto.DateOfBirth,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created patient {Id}: {Name}", patient.Id,
            $"{patient.FirstName} {patient.LastName}");

        return MapToDto(patient);
    }

    public async Task<PatientDto?> UpdateAsync(int id, UpdatePatientDto dto)
    {
        var patient = await _context.Patients
            .Include(p => p.Appointments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null) return null;

        dto.DateOfBirth = dto.DateOfBirth.ToUtc();

        // Only update fields that are provided (partial update)
        if (dto.FirstName != null) patient.FirstName = dto.FirstName.Trim();
        if (dto.LastName != null) patient.LastName = dto.LastName.Trim();
        if (dto.Email != null)
        {
            // Check for duplicate email (exclude current patient)
            var duplicate = await _context.Patients
                .AnyAsync(p => p.Email.ToLower() == dto.Email.ToLower() && p.Id != id);

            if (duplicate)
                throw new ConflictException($"A patient with email '{dto.Email}' already exists.");

            patient.Email = dto.Email.Trim().ToLower();
        }
        if (dto.Phone != null) patient.Phone = dto.Phone.Trim();
        if (dto.DateOfBirth.HasValue)
        {
            if (dto.DateOfBirth.Value >= DateTime.UtcNow)
                throw new Exceptions.ValidationException("Date of birth must be in the past.");
            patient.DateOfBirth = dto.DateOfBirth.Value;
        }

        patient.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated patient {Id}", patient.Id);

        return MapToDto(patient);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var patient = await _context.Patients
            .Include(p => p.Appointments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null) return false;

        // Check for future appointments
        var hasFutureAppointments = patient.Appointments
            .Any(a => a.StartTime > DateTime.UtcNow &&
                      a.Status != AppointmentStatus.Cancelled);

        if (hasFutureAppointments)
        {
            throw new ConflictException(
                "Cannot delete patient with upcoming appointments. Cancel appointments first.");
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted patient {Id}", id);

        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Patients.AnyAsync(p => p.Id == id);
    }

    // Private helper: Map entity to DTO
    private static PatientDto MapToDto(Patient patient)
    {
        var today = DateTime.Today;
        var age = today.Year - patient.DateOfBirth.Year;
        if (patient.DateOfBirth.Date > today.AddYears(-age)) age--;

        return new PatientDto
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            FullName = $"{patient.FirstName} {patient.LastName}",
            Email = patient.Email,
            Phone = patient.Phone,
            DateOfBirth = patient.DateOfBirth,
            Age = age,
            CreatedAt = patient.CreatedAt,
            TotalAppointments = patient.Appointments?.Count ?? 0
        };
    }
}