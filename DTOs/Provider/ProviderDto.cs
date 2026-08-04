using ClinicFlow.Api.DTOs.AvailabilitySlot;

namespace ClinicFlow.Api.DTOs.Provider;

public class ProviderDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public List<AvailabilitySlotDto> AvailabilitySlots { get; set; } = new();
}