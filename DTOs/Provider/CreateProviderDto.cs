using System.ComponentModel.DataAnnotations;

namespace ClinicFlow.Api.DTOs.Provider;

public class CreateProviderDto
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Specialty { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}