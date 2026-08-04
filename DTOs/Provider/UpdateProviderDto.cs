using System.ComponentModel.DataAnnotations;

namespace ClinicFlow.Api.DTOs.Provider;

public class UpdateProviderDto
{
    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(100)]
    public string? Specialty { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}