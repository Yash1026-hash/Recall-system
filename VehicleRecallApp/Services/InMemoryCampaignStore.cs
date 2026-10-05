using System.Net.Http.Json;
using VehicleRecall.Shared.Models;

namespace VehicleRecallApp.Services;

public class InMemoryCampaignStore
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly List<RecallCampaign> _localCache = new();

    public InMemoryCampaignStore(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<RecallCampaign> GetAll()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = client.GetAsync("/api/campaigns").GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                var campaigns = response.Content.ReadFromJsonAsync<List<RecallCampaign>>().GetAwaiter().GetResult();
                if (campaigns is not null)
                {
                    _localCache.Clear();
                    _localCache.AddRange(campaigns);
                    return campaigns;
                }
            }
        }
        catch
        {
            // Fallback to local cache if API is offline
        }

        return _localCache;
    }

    public RecallCampaign? GetById(string campaignId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = client.GetAsync($"/api/campaigns/{Uri.EscapeDataString(campaignId)}").GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                var campaign = response.Content.ReadFromJsonAsync<RecallCampaign>().GetAwaiter().GetResult();
                if (campaign is not null)
                {
                    var index = _localCache.FindIndex(c => string.Equals(c.NhtsaId, campaignId, StringComparison.OrdinalIgnoreCase));
                    if (index >= 0)
                    {
                        _localCache[index] = campaign;
                    }
                    else
                    {
                        _localCache.Add(campaign);
                    }
                    return campaign;
                }
            }
        }
        catch
        {
            // Fallback to local cache if API is offline
        }

        return _localCache.FirstOrDefault(campaign =>
            string.Equals(campaign.NhtsaId, campaignId, StringComparison.OrdinalIgnoreCase));
    }

    public CampaignSummary GetSummary()
    {
        var campaigns = GetAll();
        return new CampaignSummary
        {
            TotalCampaigns = campaigns.Count,
            ActiveCampaigns = campaigns.Count(c => c.Status == "Active"),
            TotalAffectedVins = campaigns.Sum(c => c.AffectedVins)
        };
    }

    public void Add(RecallCampaign campaign)
    {
        if (campaign.Users is null)
        {
            campaign.Users = new List<CampaignUserStatus>();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = client.PostAsJsonAsync("/api/campaigns", campaign).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                var saved = response.Content.ReadFromJsonAsync<RecallCampaign>().GetAwaiter().GetResult();
                if (saved is not null)
                {
                    campaign.Users = saved.Users;
                    campaign.AffectedVins = saved.AffectedVins;
                }
            }
        }
        catch
        {
            // Keep in local cache if API is offline
        }

        var existingIndex = _localCache.FindIndex(c => string.Equals(c.NhtsaId, campaign.NhtsaId, StringComparison.OrdinalIgnoreCase));
        if (existingIndex >= 0)
        {
            _localCache[existingIndex] = campaign;
        }
        else
        {
            _localCache.Add(campaign);
        }
    }

    public sealed class CampaignSummary
    {
        public int TotalCampaigns { get; set; }
        public int ActiveCampaigns { get; set; }
        public int TotalAffectedVins { get; set; }
    }
}