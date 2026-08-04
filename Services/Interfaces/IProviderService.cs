using ClinicFlow.Api.DTOs.Provider;
using ClinicFlow.Api.DTOs.AvailabilitySlot;

namespace ClinicFlow.Api.Services.Interfaces;

public interface IProviderService
{
    Task<List<ProviderDto>> GetAllAsync(string? specialty = null);
    Task<ProviderDto?> GetByIdAsync(int id);
    Task<ProviderDto> CreateAsync(CreateProviderDto dto);
    Task<ProviderDto?> UpdateAsync(int id, UpdateProviderDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<AvailabilitySlotDto> AddAvailabilitySlotAsync(CreateAvailabilitySlotDto dto);
    Task<List<AvailabilitySlotDto>> GetAvailabilityAsync(int providerId);
    /// <param name="excludeAppointmentId">
    /// Appointment to ignore when looking for conflicts. Pass the appointment being
    /// rescheduled so that it does not count as a conflict with itself.
    /// </param>
    Task<bool> IsAvailableAsync(
        int providerId, DateTime startTime, DateTime endTime, int? excludeAppointmentId = null);
    Task<bool> RemoveAvailabilitySlotAsync(int slotId);
}