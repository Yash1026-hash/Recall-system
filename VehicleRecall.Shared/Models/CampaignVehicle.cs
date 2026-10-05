using System.Text.Json.Serialization;

namespace VehicleRecall.Shared.Models;

public class CampaignVehicle
{
    public string CampaignId { get; set; } = string.Empty;
    public string Vin { get; set; } = string.Empty;
    public bool AppointmentBooked { get; set; }
    public bool RepairCompleted { get; set; }

    [JsonIgnore]
    public RecallCampaign? Campaign { get; set; }

    [JsonIgnore]
    public Vehicle? Vehicle { get; set; }
}
