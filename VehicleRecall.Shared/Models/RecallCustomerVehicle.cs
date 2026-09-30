using System.Text.Json.Serialization;

namespace VehicleRecall.Shared.Models;

public class RecallCustomerVehicle
{
    public int CustomerId { get; set; }

    [JsonIgnore]
    public RecallCustomer Customer { get; set; } = null!;
    public string Vin { get; set; } = string.Empty;

    [JsonIgnore]
    public Vehicle Vehicle { get; set; } = null!;
}