using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecallApp.Services;

namespace VehicleRecallApp.Pages.Manager;

public class DashboardModel : PageModel
{
    private readonly InMemoryCampaignStore _campaignStore;
    public int ActiveCampaigns { get; private set; }
    public int AffectedVins { get; private set; }
    public int ScheduledAppointments { get; private set; }
    public decimal RemediationRate { get; private set; }

    public DashboardModel(InMemoryCampaignStore campaignStore)
    {
        _campaignStore = campaignStore;
    }

    public void OnGet()
    {
        var summary = _campaignStore.GetSummary();

        ActiveCampaigns = summary.ActiveCampaigns;
        AffectedVins = summary.TotalAffectedVins;
        ScheduledAppointments = 0;
        RemediationRate = 0m;
    }
}