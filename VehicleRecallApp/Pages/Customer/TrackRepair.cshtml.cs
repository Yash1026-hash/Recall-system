using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VehicleRecallApp.Pages.Customer;

public class TrackRepairModel : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Enter your booking reference.")]
    [Display(Name = "Booking Reference")]
    public string BookingReference { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Enter your VIN.")]
    [StringLength(17, MinimumLength = 17,
        ErrorMessage = "A VIN must contain exactly 17 characters.")]
    [RegularExpression(@"^[A-HJ-NPR-Z0-9]{17}$",
        ErrorMessage = "A VIN may not contain I, O, or Q.")]
    public string Vin { get; set; } = string.Empty;

    public bool TrackingFound { get; private set; }
    public string CurrentStatus { get; private set; } = string.Empty;
    public List<RepairStep> Steps { get; private set; } = new();

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        Vin = Vin.Trim().ToUpperInvariant();
        BookingReference = BookingReference.Trim().ToUpperInvariant();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Temporary mock tracking record.
        // Later, look up the appointment using booking reference + VIN.
        if (Vin == "1HGCM82633A004352")
        {
            TrackingFound = true;
            CurrentStatus = "In Repair";

            Steps = new List<RepairStep>
            {
                new RepairStep("Scheduled", true),
                new RepairStep("Checked In", true),
                new RepairStep("In Repair", true),
                new RepairStep("Ready for Pickup", false),
                new RepairStep("Closed", false)
            };
        }

        return Page();
    }

    public class RepairStep
    {
        public RepairStep(string name, bool completed)
        {
            Name = name;
            Completed = completed;
        }

        public string Name { get; set; }
        public bool Completed { get; set; }
    }
}