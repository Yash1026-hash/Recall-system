using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VehicleRecallApp.Pages.Technician;

public class WorkOrderModel : PageModel
{
    public RecallWorkOrder WorkOrder { get; private set; } = new();

    public void OnGet(string? workOrderNumber)
    {
        WorkOrder = new RecallWorkOrder
        {
            WorkOrderNumber = string.IsNullOrWhiteSpace(workOrderNumber)
                ? "WO-10021"
                : workOrderNumber,
            Vin = "1HGCM82633A004352",
            CustomerName = "Arun Kumar",
            CampaignId = "24V-102",
            Vehicle = "Honda Accord",
            AppointmentTime = "09:00 AM",
            Bay = "Bay 1",
            Technician = "Ravi",
            Status = "Checked In"
        };
    }

    public class RecallWorkOrder
    {
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string Vin { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CampaignId { get; set; } = string.Empty;
        public string Vehicle { get; set; } = string.Empty;
        public string AppointmentTime { get; set; } = string.Empty;
        public string Bay { get; set; } = string.Empty;
        public string Technician { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}