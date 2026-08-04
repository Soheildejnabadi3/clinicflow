namespace ClinicFlow.Api.Models;

public class Provider
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public List<AvailabilitySlot> AvailabilitySlots { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();
}
