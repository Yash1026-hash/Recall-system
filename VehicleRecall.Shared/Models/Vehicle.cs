using System.Text.Json.Serialization;

namespace VehicleRecall.Shared.Models;

public class Vehicle
{
    public string Vin { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string RecallStatus { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<RecallCustomerVehicle> CustomerVehicles { get; set; } =
        new List<RecallCustomerVehicle>();
}