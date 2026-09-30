using System.ComponentModel.DataAnnotations;

namespace VehicleRecall.Shared.Models;

public class UpdateUserRoleRequest
{
    [Required]
    [RegularExpression("^(Manager|Technician|Customer)$")]
    public string Role { get; set; } = string.Empty;
}