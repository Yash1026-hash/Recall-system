using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecallApp.Models;
using VehicleRecallApp.Services;

namespace VehicleRecallApp.Pages.Manager;

public class CampaignsModel : PageModel
{
    private readonly InMemoryCampaignStore _campaignStore;

    public List<RecallCampaign> Campaigns { get; set; } = new();

    public CampaignsModel(InMemoryCampaignStore campaignStore)
    {
        _campaignStore = campaignStore;
    }

    public void OnGet()
    {
        Campaigns = _campaignStore.GetAll();
    }
}