namespace ClinicFlow.Api.DTOs.AvailabilitySlot;

public class AvailabilitySlotDto
{
    public int Id { get; set; }
    public int ProviderId { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}