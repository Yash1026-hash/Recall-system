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
            AffectedVins = 12480,
            Users = new List<CampaignUserStatus>
            {
                new() { CustomerName = "Alicia Patel", Email = "alicia.patel@example.com", Vin = "1HGBH41JXMN109186", AppointmentBooked = true, RepairCompleted = false },
                new() { CustomerName = "Marcus Lee", Email = "marcus.lee@example.com", Vin = "2T2BK1BA6KC123456", AppointmentBooked = false, RepairCompleted = false },
                new() { CustomerName = "Nora Singh", Email = "nora.singh@example.com", Vin = "3VW2K7AJ5PM100123", AppointmentBooked = true, RepairCompleted = true }
            }
        },
        new RecallCampaign
        {
            NhtsaId = "24V-223",
            Description = "Brake booster recall",
            AffectedComponent = "Brake Booster",
            Severity = "Critical",
            AffectedVins = 8605,
            Status = "Monitoring",
            Users = new List<CampaignUserStatus>
            {
                new() { CustomerName = "Daniel Brooks", Email = "daniel.brooks@example.com", Vin = "JH4KA9650LC012345", AppointmentBooked = true, RepairCompleted = false },
                new() { CustomerName = "Emma Davis", Email = "emma.davis@example.com", Vin = "1C4RJFBG1MC289142", AppointmentBooked = false, RepairCompleted = false },
                new() { CustomerName = "Rafael Gomez", Email = "rafael.gomez@example.com", Vin = "WAUZZZ8K3FA123456", AppointmentBooked = true, RepairCompleted = true }
            }
        }
    };

    public List<RecallCampaign> GetAll()
    {
        return _campaigns;
    }

    public RecallCampaign? GetById(string campaignId) =>
        _campaigns.FirstOrDefault(campaign =>
            string.Equals(campaign.NhtsaId, campaignId, StringComparison.OrdinalIgnoreCase));

    public CampaignSummary GetSummary()
    {
        return new CampaignSummary
        {
            TotalCampaigns = _campaigns.Count,
            ActiveCampaigns = _campaigns.Count(c => c.Status == "Active"),
            TotalAffectedVins = _campaigns.Sum(c => c.AffectedVins)
        };
    }

    public void Add(RecallCampaign campaign)
    {
        if (campaign.Users is null)
        {
            campaign.Users = new List<CampaignUserStatus>();
        }

        _campaigns.Add(campaign);
    }

    public sealed class CampaignSummary
    {
        public int TotalCampaigns { get; set; }
        public int ActiveCampaigns { get; set; }
        public int TotalAffectedVins { get; set; }
    }
}