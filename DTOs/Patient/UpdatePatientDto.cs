using System.ComponentModel.DataAnnotations;

namespace ClinicFlow.Api.DTOs.Patient;

public class UpdatePatientDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? FirstName { get; set; }

    [StringLength(100, MinimumLength = 1)]
    public string? LastName { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? Phone { get; set; }

    public DateTime? DateOfBirth { get; set; }
}