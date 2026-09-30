using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecallApp.Services;

namespace VehicleRecallApp.Pages.Manager;

public class ReportsModel : PageModel
{
    private readonly InMemoryCampaignStore _campaignStore;

    public int TotalCampaigns { get; private set; }
    public int ActiveCampaigns { get; private set; }
    public int TotalAffectedVins { get; private set; }

    public ReportsModel(InMemoryCampaignStore campaignStore)
    {
        _campaignStore = campaignStore;
    }

    public void OnGet()
    {
        var summary = _campaignStore.GetSummary();

        TotalCampaigns = summary.TotalCampaigns;
        ActiveCampaigns = summary.ActiveCampaigns;
        TotalAffectedVins = summary.TotalAffectedVins;
    }
}