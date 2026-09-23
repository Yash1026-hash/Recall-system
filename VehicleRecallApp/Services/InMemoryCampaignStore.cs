using VehicleRecallApp.Models;

namespace VehicleRecallApp.Services;

public class InMemoryCampaignStore
{
    private readonly List<RecallCampaign> _campaigns = new()
    {
        new RecallCampaign
        {
            NhtsaId = "24V-102",
            Description = "Fuel pump failure risk",
            AffectedComponent = "Fuel Pump",
            Severity = "High",
            AffectedVins = 12480
        }
    };

    public List<RecallCampaign> GetAll()
    {
        return _campaigns;
    }

    public void Add(RecallCampaign campaign)
    {
        _campaigns.Add(campaign);
    }
}