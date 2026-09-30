using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecallApp.Models;
using VehicleRecallApp.Services;

namespace VehicleRecallApp.Pages.Manager;

public class NotificationsModel : PageModel
{
    private readonly InMemoryCampaignStore _campaignStore;

    public List<RecallCampaign> Campaigns { get; private set; } = new();
    public RecallCampaign? Campaign { get; private set; }

    [BindProperty]
    public NotificationInput Input { get; set; } = new();

    public NotificationsModel(InMemoryCampaignStore campaignStore)
    {
        _campaignStore = campaignStore;
    }

    public IActionResult OnGet(string campaignId)
    {
        Campaigns = _campaignStore.GetAll();
        Campaign = _campaignStore.GetById(campaignId);
        if (Campaign is null)
        {
            return NotFound();
        }

        Input.CampaignId = Campaign.NhtsaId;
        return Page();
    }

    public IActionResult OnPost(string campaignId)
    {
        Campaigns = _campaignStore.GetAll();
        Campaign = _campaignStore.GetById(campaignId);
        if (Campaign is null)
        {
            return NotFound();
        }

        Input.CampaignId = Campaign.NhtsaId;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        TempData["SuccessMessage"] =
            $"Notifications for campaign {Input.CampaignId} were queued successfully.";

        return RedirectToPage(new { campaignId = Campaign.NhtsaId });
    }

    public class NotificationInput
    {
        [Required]
        [Display(Name = "Recall Campaign")]
        public string CampaignId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Notification Channel")]
        public string Channel { get; set; } = string.Empty;

        [Display(Name = "Geographic Filter")]
        public string Geography { get; set; } = "All regions";

        [Display(Name = "Notification Status")]
        public string NotificationStatus { get; set; } = "Not notified";

        [Required]
        [Display(Name = "Message Template")]
        public string MessageTemplate { get; set; } = string.Empty;
    }
}