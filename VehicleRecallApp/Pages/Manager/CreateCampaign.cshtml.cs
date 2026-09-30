using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecallApp.Models;
using VehicleRecallApp.Services;

namespace VehicleRecallApp.Pages.Manager;

public class CreateCampaignModel : PageModel
{
    private readonly InMemoryCampaignStore _campaignStore;
    private readonly CampaignCsvImporter _csvImporter;
    private readonly CustomerRosterSyncService _rosterSyncService;

    public CreateCampaignModel(
        InMemoryCampaignStore campaignStore,
        CampaignCsvImporter csvImporter,
        CustomerRosterSyncService rosterSyncService)
    {
        _campaignStore = campaignStore;
        _csvImporter = csvImporter;
        _rosterSyncService = rosterSyncService;
    }

    [BindProperty]
    public IFormFile? CsvFile { get; set; }

    [BindProperty]
    public CampaignInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (CsvFile is null || CsvFile.Length == 0)
        {
            ModelState.AddModelError(nameof(CsvFile), "Choose the affected-customer CSV file.");
        }
        else if (!Path.GetExtension(CsvFile.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(CsvFile), "Only CSV files are supported.");
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

        var importError = _csvImporter.Import(
            campaign,
            CsvFile!.OpenReadStream(),
            out var importedCustomers);
        if (importError is not null)
        {
            ModelState.AddModelError(nameof(CsvFile), importError);
            return Page();
        }

        var rosterSyncError = await _rosterSyncService.SyncAsync(importedCustomers);
        if (rosterSyncError is not null)
        {
            ModelState.AddModelError(nameof(CsvFile), rosterSyncError);
            return Page();
        }

        _campaignStore.Add(campaign);

        TempData["SuccessMessage"] =
            $"Campaign {campaign.NhtsaId} was created. Imported {campaign.SuccessfulImports} VIN records; {campaign.FailedImports} need attention.";

        return RedirectToPage("/Manager/VinImport", new { campaignId = campaign.NhtsaId });
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