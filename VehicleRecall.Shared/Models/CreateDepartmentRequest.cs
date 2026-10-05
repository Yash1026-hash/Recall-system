using System.ComponentModel.DataAnnotations;

namespace VehicleRecall.Shared.Models;

/// <summary>Payload for creating a department.</summary>
public sealed record CreateDepartmentRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public required string Name { get; init; }

    [StringLength(500)]
    public string Description { get; init; } = string.Empty;
}
