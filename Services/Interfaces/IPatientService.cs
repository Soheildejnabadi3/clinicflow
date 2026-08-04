using ClinicFlow.Api.DTOs.Patient;

namespace ClinicFlow.Api.Services.Interfaces;

public interface IPatientService
{
    Task<List<PatientDto>> GetAllAsync(string? searchTerm = null);
    Task<PatientDto?> GetByIdAsync(int id);
    Task<PatientDto> CreateAsync(CreatePatientDto dto);
    Task<PatientDto?> UpdateAsync(int id, UpdatePatientDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}