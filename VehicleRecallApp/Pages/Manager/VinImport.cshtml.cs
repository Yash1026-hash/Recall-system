using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VehicleRecallApp.Pages.Manager;

public class VinImportModel : PageModel
{
    public string CampaignId { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }

    public List<ImportError> Errors { get; set; } = new();

    public void OnGet()
    {
        // Temporary mock import result.
        CampaignId = "24V-102";
        TotalRecords = 1500;
        SuccessfulImports = 1487;
        FailedImports = 13;

        Errors = new List<ImportError>
        {
            new ImportError
            {
                RowNumber = 45,
                Vin = "1HGCM82633A00435",
                Reason = "VIN must contain exactly 17 characters."
            },
            new ImportError
            {
                RowNumber = 128,
                Vin = "1HGCM826I3A004352",
                Reason = "VIN cannot contain the letter I."
            },
            new ImportError
            {
                RowNumber = 301,
                Vin = "INVALIDVIN1234567",
                Reason = "VIN checksum validation failed."
            }
        };
    }

    public class ImportError
    {
        public int RowNumber { get; set; }
        public string Vin { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}