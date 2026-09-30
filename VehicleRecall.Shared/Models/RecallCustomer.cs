using System.Text.Json.Serialization;

namespace VehicleRecall.Shared.Models;

public class RecallCustomer
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public DateTime UpdatedAt { get; set; }

    [JsonIgnore]
    public ICollection<RecallCustomerVehicle> Vehicles { get; set; } =
        new List<RecallCustomerVehicle>();

    [JsonIgnore]
    public UserAccount? UserAccount { get; set; }
}