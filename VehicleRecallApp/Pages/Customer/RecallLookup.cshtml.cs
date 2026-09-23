using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VehicleRecallApp.Pages.Customer;

public class RecallLookupModel : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Enter a VIN.")]
    [StringLength(17, MinimumLength = 17,
        ErrorMessage = "A VIN must contain exactly 17 characters.")]
    [RegularExpression(@"^[A-HJ-NPR-Z0-9]{17}$",
        ErrorMessage = "A VIN may contain letters and numbers, but not I, O, or Q.")]
    public string Vin { get; set; } = string.Empty;

    public bool SearchCompleted { get; private set; }
    public RecallAdvisory? ActiveRecall { get; private set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        Vin = Vin.Trim().ToUpperInvariant();
        SearchCompleted = true;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Temporary mock lookup.
        // Later, this queries the Recall_Vehicles database table.
        if (Vin == "1HGCM82633A004352")
        {
            ActiveRecall = new RecallAdvisory
            {
                CampaignId = "24V-102",
                DefectDescription = "Fuel pump failure risk",
                SafetyHazard = "A sudden loss of engine power may increase crash risk.",
                Remedy = "An authorized service center will replace the fuel pump at no cost."
            };
        }

        return Page();
    }

    public class RecallAdvisory
    {
        public string CampaignId { get; set; } = string.Empty;
        public string DefectDescription { get; set; } = string.Empty;
        public string SafetyHazard { get; set; } = string.Empty;
        public string Remedy { get; set; } = string.Empty;
    }
}