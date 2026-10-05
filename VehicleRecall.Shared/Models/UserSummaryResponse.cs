namespace VehicleRecall.Shared.Models;

public sealed record UserSummaryResponse(
    int UserId,
    string Username,
    string Role,
    string FullName,
    string Email,
    string RegistrationStatus)
{
    public int? CustomerId { get; init; }
    public int DepartmentId { get; init; }
    public string Department { get; init; } = "Operations";
    public bool IsActive { get; init; } = true;
    public DateTime? CreatedDate { get; init; }
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
        CustomerId = user.CustomerId,
        DepartmentId = user.DepartmentId,
        Department = user.Department.Name,
        IsActive = user.IsActive,
        CreatedDate = user.CreatedAt,
        Profile = new UserProfileResponse(
            user.Username,
            user.FullName,
            user.Email,
            user.CustomerId,
            user.Customer?.Address,
            user.Customer?.City,
            user.Customer?.State,
            user.Customer?.PostalCode),
        Roles = user.UserRoles
            .Select(userRole => new RoleResponse(userRole.Role.RoleId, userRole.Role.Name))
            .ToArray()
    };
}

public sealed record UserProfileResponse(
    string Username,
    string FullName,
    string Email,
    int? CustomerId = null,
    string? Address = null,
    string? City = null,
    string? State = null,
    string? PostalCode = null);

public sealed record RoleResponse(int RoleId, string Name);
