namespace ClinicFlow.Api.DTOs.Appointment;

public class UpdateAppointmentDto
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}