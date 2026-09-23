namespace VehicleRecallApp.Models;

public class RecallCampaign
{
    public string NhtsaId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RemedyInstructions { get; set; } = string.Empty;
    public string AffectedComponent { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public int AffectedVins { get; set; }
}