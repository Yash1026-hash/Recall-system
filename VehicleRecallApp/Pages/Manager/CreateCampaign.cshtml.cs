using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecallApp.Models;
using VehicleRecallApp.Services;

namespace VehicleRecallApp.Pages.Manager;

public class CreateCampaignModel : PageModel
{
    private readonly InMemoryCampaignStore _campaignStore;

    public CreateCampaignModel(InMemoryCampaignStore campaignStore)
    {
        _campaignStore = campaignStore;
    }

    [BindProperty]
    public IFormFile? VinCsvFile { get; set; }

    [BindProperty]
    public CampaignInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (VinCsvFile is null || VinCsvFile.Length == 0)
        {
            ModelState.AddModelError("VinCsvFile", "Please upload a CSV file.");
        }
        else if (!Path.GetExtension(VinCsvFile.FileName)
                     .Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("VinCsvFile", "Only .csv files are allowed.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var campaign = new RecallCampaign
        {
            NhtsaId = Input.NhtsaId,
            Description = Input.DefectDescription,
            AffectedComponent = Input.AffectedComponent,
            RemedyInstructions = Input.RemedyInstructions,
            Severity = Input.SeverityTier,
            Status = "Active",

            // The CSV parser will set this later.
            AffectedVins = 0
        };

        _campaignStore.Add(campaign);

        TempData["SuccessMessage"] =
            $"Campaign {campaign.NhtsaId} was created successfully.";

        return RedirectToPage("/Manager/Campaigns");
    }

    public class CampaignInput
    {
        [Required(ErrorMessage = "NHTSA campaign ID is required.")]
        [Display(Name = "NHTSA Campaign ID")]
        public string NhtsaId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Defect description is required.")]
        [Display(Name = "Defect Description")]
        public string DefectDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Affected component is required.")]
        [Display(Name = "Affected Component")]
        public string AffectedComponent { get; set; } = string.Empty;

        [Required(ErrorMessage = "Remedy instructions are required.")]
        [Display(Name = "Remedy Instructions")]
        public string RemedyInstructions { get; set; } = string.Empty;

        [Required(ErrorMessage = "Select a severity tier.")]
        [Display(Name = "Severity Tier")]
        public string SeverityTier { get; set; } = string.Empty;
    }
}