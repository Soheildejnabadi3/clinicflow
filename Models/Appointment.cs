namespace ClinicFlow.Api.Models;

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int ProviderId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public Provider Provider { get; set; } = null!;
}

public enum AppointmentStatus
{
    Scheduled,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    NoShow
}