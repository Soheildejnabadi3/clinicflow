using System.ComponentModel.DataAnnotations;

namespace ClinicFlow.Api.DTOs.Appointment;

public class CreateAppointmentDto
{
    [Required]
    public int PatientId { get; set; }

    [Required]
    public int ProviderId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    public string? Notes { get; set; }
}