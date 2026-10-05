using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecallApp.Models;
using VehicleRecallApp.Services;

namespace VehicleRecallApp.Pages.Manager;

public class VinImportModel : PageModel
{
    private readonly InMemoryCampaignStore _campaignStore;
    private readonly CampaignCsvImporter _csvImporter;
    private readonly CustomerRosterSyncService _rosterSyncService;

    [BindProperty]
    public IFormFile? CsvFile { get; set; }

    public string CampaignId { get; private set; } = string.Empty;
    public int TotalRecords { get; private set; }
    public int SuccessfulImports { get; private set; }
    public int FailedImports { get; private set; }
    public List<CampaignImportError> Errors { get; private set; } = new();

    public VinImportModel(
        InMemoryCampaignStore campaignStore,
        CampaignCsvImporter csvImporter,
        CustomerRosterSyncService rosterSyncService)
    {
        _campaignStore = campaignStore;
        _csvImporter = csvImporter;
        _rosterSyncService = rosterSyncService;
    }

    public IActionResult OnGet(string campaignId)
    {
        var campaign = _campaignStore.GetById(campaignId);
        if (campaign is null)
        {
            return NotFound();
        }

        LoadResults(campaign);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string campaignId)
    {
        var campaign = _campaignStore.GetById(campaignId);
        if (campaign is null)
        {
            return NotFound();
        }

        if (CsvFile is null || CsvFile.Length == 0)
        {
            ModelState.AddModelError(nameof(CsvFile), "Choose a CSV file to import.");
        }
        else if (!Path.GetExtension(CsvFile.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(CsvFile), "Only CSV files are supported.");
        }

        if (!ModelState.IsValid)
        {
            LoadResults(campaign);
            return Page();
        }

        var importError = _csvImporter.Import(
            campaign,
            CsvFile!.OpenReadStream(),
            out var importedCustomers);
        if (importError is not null)
        {
            ModelState.AddModelError(nameof(CsvFile), importError);
            LoadResults(campaign);
            return Page();
        }

        if (importedCustomers.Count > 0)
        {
            var rosterSyncError = await _rosterSyncService.SyncAsync(importedCustomers);
            if (rosterSyncError is not null)
            {
                ModelState.AddModelError(nameof(CsvFile), rosterSyncError);
                LoadResults(campaign);
                return Page();
            }

            _campaignStore.Add(campaign);
        }

        TempData["SuccessMessage"] =
            $"Imported {campaign.SuccessfulImports} VIN records for campaign {campaign.NhtsaId}; customer emails are now eligible for registration.";
        return RedirectToPage(new { campaignId = campaign.NhtsaId });
    }

    private void LoadResults(RecallCampaign campaign)
    {
        CampaignId = campaign.NhtsaId;
        TotalRecords = campaign.ImportedRecords;
        SuccessfulImports = campaign.SuccessfulImports;
        FailedImports = campaign.FailedImports;
        Errors = campaign.ImportErrors;
    }

}
