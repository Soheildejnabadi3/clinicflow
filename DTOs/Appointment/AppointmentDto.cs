namespace ClinicFlow.Api.DTOs.Appointment;

public class AppointmentDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderSpecialty { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}