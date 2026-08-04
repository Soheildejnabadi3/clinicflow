namespace ClinicFlow.Api.Models;

public class AvailabilitySlot
{
    public int Id { get; set; }
    public int ProviderId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; } = true;

    public Provider Provider { get; set; } = null!;
}