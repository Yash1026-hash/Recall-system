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

    [BindProperty]
    public NotificationInput Input { get; set; } = new();

    public NotificationsModel(InMemoryCampaignStore campaignStore)
    {
        _campaignStore = campaignStore;
    }

    public void OnGet()
    {
        Campaigns = _campaignStore.GetAll();
    }

    public IActionResult OnPost()
    {
        Campaigns = _campaignStore.GetAll();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        TempData["SuccessMessage"] =
            $"Notifications for campaign {Input.CampaignId} were queued successfully.";

        return RedirectToPage();
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