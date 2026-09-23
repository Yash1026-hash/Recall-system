using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecallApp.Services;

namespace VehicleRecallApp.Pages.Manager;

public class DashboardModel : PageModel
{
    private readonly InMemoryCampaignStore _campaignStore;

    public int ActiveCampaigns { get; private set; }
    public int AffectedVins { get; private set; }

    // These stay as frontend mock values until appointment and repair data exists.
    public int ScheduledAppointments { get; private set; }
    public decimal RemediationRate { get; private set; }

    public DashboardModel(InMemoryCampaignStore campaignStore)
    {
        _campaignStore = campaignStore;
    }

    public void OnGet()
    {
        var campaigns = _campaignStore.GetAll();

        ActiveCampaigns = campaigns.Count(c => c.Status == "Active");
        AffectedVins = campaigns.Sum(c => c.AffectedVins);

        ScheduledAppointments = 0;
        RemediationRate = 0m;
    }
}