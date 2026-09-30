using System.Text.Json.Serialization;

namespace VehicleRecall.Shared.Models;

public class UserAccount
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    [JsonIgnore]
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RegistrationStatus { get; set; } = "Pending";
    public int? CustomerId { get; set; }

    [JsonIgnore]
    public RecallCustomer? Customer { get; set; }

    [JsonIgnore]
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}