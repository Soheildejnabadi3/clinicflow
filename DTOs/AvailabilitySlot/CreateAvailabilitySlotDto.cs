using System.ComponentModel.DataAnnotations;

namespace ClinicFlow.Api.DTOs.AvailabilitySlot;

public class CreateAvailabilitySlotDto
{
    [Required]
    public int ProviderId { get; set; }

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }
}