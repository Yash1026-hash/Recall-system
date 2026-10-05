using System.ComponentModel.DataAnnotations;

namespace VehicleRecall.Shared.Models;

/// <summary>Payload for updating a user's editable profile fields.</summary>
public sealed record UpdateUserRequest
{
    [Required, StringLength(30, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_.]+$")]
    public required string Username { get; init; }

    [Required, StringLength(100, MinimumLength = 2)]
    public required string FullName { get; init; }

    [Required, EmailAddress, StringLength(254)]
    public required string Email { get; init; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; init; }
}
