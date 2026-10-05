using System.ComponentModel.DataAnnotations;

namespace VehicleRecall.Shared.Models;

/// <summary>Payload for replacing a user's role assignments.</summary>
public sealed record UpdateUserRolesRequest
{
    [Required, MinLength(1)]
    public required IReadOnlyList<string> Roles { get; init; }
}
