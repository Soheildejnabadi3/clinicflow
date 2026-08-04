using Microsoft.EntityFrameworkCore;
using ClinicFlow.Api.Data;
using ClinicFlow.Api.DTOs.Provider;
using ClinicFlow.Api.DTOs.AvailabilitySlot;
using ClinicFlow.Api.Exceptions;
using ClinicFlow.Api.Models;
using ClinicFlow.Api.Services.Interfaces;

namespace ClinicFlow.Api.Services;

public class ProviderService : IProviderService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProviderService> _logger;

    public ProviderService(AppDbContext context, ILogger<ProviderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ProviderDto>> GetAllAsync(string? specialty = null)
    {
        var query = _context.Providers
            .Include(p => p.Appointments)
            .Include(p => p.AvailabilitySlots)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialty))
        {
            query = query.Where(p =>
                p.Specialty.ToLower().Contains(specialty.ToLower()));
        }

        var providers = await query
            .OrderBy(p => p.LastName)
            .ToListAsync();

        return providers.Select(MapToDto).ToList();
    }

    public async Task<ProviderDto?> GetByIdAsync(int id)
    {
        var provider = await _context.Providers
            .Include(p => p.Appointments)
            .Include(p => p.AvailabilitySlots)
            .FirstOrDefaultAsync(p => p.Id == id);

        return provider == null ? null : MapToDto(provider);
    }

    public async Task<ProviderDto> CreateAsync(CreateProviderDto dto)
    {
        var duplicate = await _context.Providers
            .AnyAsync(p => p.Email.ToLower() == dto.Email.ToLower());

        if (duplicate)
            throw new ConflictException($"A provider with email '{dto.Email}' already exists.");

        var provider = new Provider
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Specialty = dto.Specialty.Trim(),
            Email = dto.Email.Trim().ToLower()
        };

        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created provider {Id}: Dr. {Name}", provider.Id,
            $"{provider.FirstName} {provider.LastName}");

        return MapToDto(provider);
    }

    public async Task<ProviderDto?> UpdateAsync(int id, UpdateProviderDto dto)
    {
        var provider = await _context.Providers
            .Include(p => p.Appointments)
            .Include(p => p.AvailabilitySlots)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (provider == null) return null;

        if (dto.FirstName != null) provider.FirstName = dto.FirstName.Trim();
        if (dto.LastName != null) provider.LastName = dto.LastName.Trim();
        if (dto.Specialty != null) provider.Specialty = dto.Specialty.Trim();
        if (dto.Email != null)
        {
            var duplicate = await _context.Providers
                .AnyAsync(p => p.Email.ToLower() == dto.Email.ToLower() && p.Id != id);
            if (duplicate)
                throw new ConflictException($"A provider with email '{dto.Email}' already exists.");
            provider.Email = dto.Email.Trim().ToLower();
        }

        await _context.SaveChangesAsync();
        return MapToDto(provider);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var provider = await _context.Providers
            .Include(p => p.Appointments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (provider == null) return false;

        var hasFutureAppointments = provider.Appointments
            .Any(a => a.StartTime > DateTime.UtcNow &&
                      a.Status != AppointmentStatus.Cancelled);

        if (hasFutureAppointments)
            throw new ConflictException("Cannot delete provider with upcoming appointments.");

        _context.Providers.Remove(provider);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Providers.AnyAsync(p => p.Id == id);
    }

    public async Task<AvailabilitySlotDto> AddAvailabilitySlotAsync(CreateAvailabilitySlotDto dto)
    {
        if (!await ExistsAsync(dto.ProviderId))
            throw new NotFoundException("Provider", dto.ProviderId);

        if (dto.StartTime >= dto.EndTime)
            throw new Exceptions.ValidationException("Start time must be before end time.");

        // Check for overlapping slots on the same day
        var overlapping = await _context.AvailabilitySlots
            .AnyAsync(s =>
                s.ProviderId == dto.ProviderId &&
                s.DayOfWeek == dto.DayOfWeek &&
                s.IsActive &&
                s.StartTime < dto.EndTime &&
                s.EndTime > dto.StartTime);

        if (overlapping)
            throw new ConflictException("This time slot overlaps with an existing availability slot.");

        var slot = new AvailabilitySlot
        {
            ProviderId = dto.ProviderId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            IsActive = true
        };

        _context.AvailabilitySlots.Add(slot);
        await _context.SaveChangesAsync();

        return MapSlotToDto(slot);
    }

    public async Task<List<AvailabilitySlotDto>> GetAvailabilityAsync(int providerId)
    {
        var slots = await _context.AvailabilitySlots
            .Where(s => s.ProviderId == providerId && s.IsActive)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        return slots.Select(MapSlotToDto).ToList();
    }

    public async Task<bool> IsAvailableAsync(
        int providerId, DateTime startTime, DateTime endTime, int? excludeAppointmentId = null)
    {
        var dayOfWeek = startTime.DayOfWeek;
        var requestedStart = TimeOnly.FromDateTime(startTime);
        var requestedEnd = TimeOnly.FromDateTime(endTime);

        // Check if provider has an availability slot covering this time
        var hasSlot = await _context.AvailabilitySlots.AnyAsync(s =>
            s.ProviderId == providerId &&
            s.DayOfWeek == dayOfWeek &&
            s.IsActive &&
            s.StartTime <= requestedStart &&
            s.EndTime >= requestedEnd);

        if (!hasSlot) return false;

        // Check for conflicting appointments
        var hasConflict = await _context.Appointments.AnyAsync(a =>
            a.ProviderId == providerId &&
            a.Id != excludeAppointmentId &&
            a.Status != AppointmentStatus.Cancelled &&
            a.StartTime < endTime &&
            a.EndTime > startTime);

        return !hasConflict;
    }

    public async Task<bool> RemoveAvailabilitySlotAsync(int slotId)
    {
        var slot = await _context.AvailabilitySlots.FindAsync(slotId);
        if (slot == null) return false;

        slot.IsActive = false;  // Soft delete
        await _context.SaveChangesAsync();
        return true;
    }

    private static ProviderDto MapToDto(Provider provider)
    {
        return new ProviderDto
        {
            Id = provider.Id,
            FirstName = provider.FirstName,
            LastName = provider.LastName,
            FullName = $"Dr. {provider.FirstName} {provider.LastName}",
            Specialty = provider.Specialty,
            Email = provider.Email,
            TotalAppointments = provider.Appointments?.Count ?? 0,
            AvailabilitySlots = provider.AvailabilitySlots?
                .Where(s => s.IsActive)
                .Select(MapSlotToDto)
                .ToList() ?? new()
        };
    }

    private static AvailabilitySlotDto MapSlotToDto(AvailabilitySlot slot)
    {
        return new AvailabilitySlotDto
        {
            Id = slot.Id,
            ProviderId = slot.ProviderId,
            DayOfWeek = slot.DayOfWeek.ToString(),
            StartTime = slot.StartTime.ToString("HH:mm"),
            EndTime = slot.EndTime.ToString("HH:mm"),
            IsActive = slot.IsActive
        };
    }
}