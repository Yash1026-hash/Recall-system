using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecall.Shared.Models;
using VehicleRecallApp.Services;

namespace VehicleRecallApp.Pages.Manager;

public class ReportsModel : PageModel
{
    private readonly InMemoryCampaignStore _campaignStore;
    private readonly IHttpClientFactory _httpClientFactory;

    public int TotalCampaigns { get; private set; }
    public int ActiveCampaigns { get; private set; }
    public int TotalAffectedVins { get; private set; }
    public AggregatedReportResponse AggregatedReports { get; private set; } = new();

    public ReportsModel(InMemoryCampaignStore campaignStore, IHttpClientFactory httpClientFactory)
    {
        _campaignStore = campaignStore;
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnGetAsync()
    {
        var summary = _campaignStore.GetSummary();

        TotalCampaigns = summary.TotalCampaigns;
        ActiveCampaigns = summary.ActiveCampaigns;
        TotalAffectedVins = summary.TotalAffectedVins;

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("/api/reports/aggregated");
            if (response.IsSuccessStatusCode)
            {
                AggregatedReports = await response.Content.ReadFromJsonAsync<AggregatedReportResponse>() ?? new();
            }
        }
        catch
        {
            // API may be offline or unreachable; fallback gracefully
        }
    }
}
