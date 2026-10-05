using System.ComponentModel.DataAnnotations;

namespace VehicleRecall.Shared.Models;

public class StaffUserRequest
{
    [Required]
    [StringLength(30, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_.]+$")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Manager|Technician)$")]
    public string Role { get; set; } = "Technician";

    [StringLength(100)]
    public string? Department { get; set; }

    [Range(0, int.MaxValue)]
    public int DepartmentId { get; set; }
}