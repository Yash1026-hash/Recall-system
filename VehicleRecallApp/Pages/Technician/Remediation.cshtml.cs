using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VehicleRecallApp.Pages.Technician;

public class RemediationModel : PageModel
{
    public RecallRemediation Remediation { get; private set; } = new();

    [BindProperty]
    public SignOffInput Input { get; set; } = new();

    public bool SignedOff { get; private set; }

    public string? ErrorMessage { get; private set; }

    public void OnGet(string? workOrderNumber)
    {
        LoadRemediation(workOrderNumber);
    }

    public void OnPost()
    {
        if (!Input.RepairPerformed || !Input.QualityCheck)
        {
            ErrorMessage = "Confirm both repair completion and the quality check before signing off.";
            LoadRemediation(Input.WorkOrderNumber);
            return;
        }

        LoadRemediation(Input.WorkOrderNumber);
        SignedOff = true;
    }

    private void LoadRemediation(string? workOrderNumber)
    {
        Remediation = new RecallRemediation
        {
            WorkOrderNumber = string.IsNullOrWhiteSpace(workOrderNumber)
                ? "WO-10021"
                : workOrderNumber,
            Vin = "1HGCM82633A004352",
            CustomerName = "Arun Kumar",
            CampaignId = "24V-102",
            RepairDescription = "Replace the affected recall component and verify proper operation.",
            TechnicianName = "Ravi"
        };
    }

    public class SignOffInput
    {
        public string WorkOrderNumber { get; set; } = string.Empty;
        public bool RepairPerformed { get; set; }
        public bool QualityCheck { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class RecallRemediation
    {
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string Vin { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CampaignId { get; set; } = string.Empty;
        public string RepairDescription { get; set; } = string.Empty;
        public string TechnicianName { get; set; } = string.Empty;
    }
}