using ClinicFlow.Api.DTOs.Appointment;

namespace ClinicFlow.Api.Services.Interfaces;

public interface IAppointmentService
{
    Task<List<AppointmentDto>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<AppointmentDto?> GetByIdAsync(int id);
    Task<List<AppointmentDto>> GetByPatientIdAsync(int patientId);
    Task<List<AppointmentDto>> GetByProviderIdAsync(int providerId, DateTime? date = null);
    Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto);
    Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto dto);
    Task<AppointmentDto?> CancelAsync(int id, string reason);
    Task<AppointmentDto?> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}

// Dashboard stats DTO (bonus feature)
public class DashboardStatsDto
{
    public int TotalPatients { get; set; }
    public int TotalProviders { get; set; }
    public int TodayAppointments { get; set; }
    public int UpcomingAppointments { get; set; }
    public int CancelledThisWeek { get; set; }
    public int CompletedThisWeek { get; set; }
    public List<AppointmentDto> NextFiveAppointments { get; set; } = new();
}