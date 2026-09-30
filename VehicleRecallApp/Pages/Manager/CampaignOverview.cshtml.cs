using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecallApp.Models;
using VehicleRecallApp.Services;

namespace VehicleRecallApp.Pages.Manager;

public class CampaignOverviewModel : PageModel
{
    private readonly InMemoryCampaignStore _campaignStore;

    public RecallCampaign Campaign { get; private set; } = new();
    public List<CampaignUserStatus> Users { get; private set; } = new();
    public int BookedAppointments { get; private set; }
    public int RepairedVehicles { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string AppointmentStatus { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string RepairStatus { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string Sort { get; set; } = "customer";

    public CampaignOverviewModel(InMemoryCampaignStore campaignStore)
    {
        _campaignStore = campaignStore;
    }

    public IActionResult OnGet(string campaignId)
    {
        var campaign = _campaignStore.GetById(campaignId);
        if (campaign is null)
        {
            return NotFound();
        }

        Campaign = campaign;
        BookedAppointments = campaign.BookedAppointments;
        RepairedVehicles = campaign.RepairedVehicles;

        IEnumerable<CampaignUserStatus> users = campaign.Users;
        if (!string.IsNullOrWhiteSpace(Search))
        {
            users = users.Where(user =>
                user.CustomerName.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                user.Email.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                user.Vin.Contains(Search, StringComparison.OrdinalIgnoreCase));
        }

        users = AppointmentStatus switch
        {
            "booked" => users.Where(user => user.AppointmentBooked),
            "not-booked" => users.Where(user => !user.AppointmentBooked),
            _ => users
        };

        users = RepairStatus switch
        {
            "repaired" => users.Where(user => user.RepairCompleted),
            "not-repaired" => users.Where(user => !user.RepairCompleted),
            _ => users
        };

        Users = (Sort.ToLowerInvariant() switch
        {
            "vin" => users.OrderBy(user => user.Vin),
            "appointment" => users.OrderBy(user => user.AppointmentBooked).ThenBy(user => user.CustomerName),
            "repair" => users.OrderBy(user => user.RepairCompleted).ThenBy(user => user.CustomerName),
            _ => users.OrderBy(user => user.CustomerName)
        }).ToList();

        return Page();
    }
}
