namespace VehicleRecall.Shared.Models;

public sealed record UserSummaryResponse(
    int UserId,
    string Username,
    string Role,
    string FullName,
    string Email,
    string RegistrationStatus)
{
    public UserProfileResponse Profile { get; init; } = new(string.Empty, string.Empty, string.Empty);
    public IReadOnlyList<RoleResponse> Roles { get; init; } = Array.Empty<RoleResponse>();

    public static UserSummaryResponse From(UserAccount user) => new(
        user.UserId,
        user.Username,
        user.Role,
        user.FullName,
        user.Email,
        user.RegistrationStatus)
    {
        Profile = new UserProfileResponse(user.Username, user.FullName, user.Email),
        Roles = user.UserRoles
            .Select(userRole => new RoleResponse(userRole.Role.RoleId, userRole.Role.Name))
            .ToArray()
    };
}

public sealed record UserProfileResponse(string Username, string FullName, string Email);

public sealed record RoleResponse(int RoleId, string Name);
