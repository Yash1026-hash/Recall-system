using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VehicleRecallApp.Pages.Technician;

public class ServiceQueueModel : PageModel
{
    public List<RecallJob> Jobs { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string BayFilter { get; set; } = "All Bays";

    [BindProperty(SupportsGet = true)]
    public string TechnicianFilter { get; set; } = "All Technicians";

    [BindProperty(SupportsGet = true)]
    public DateTime QueueDate { get; set; } = DateTime.Today;

    public void OnGet()
    {
        var allJobs = new List<RecallJob>
        {
            new RecallJob
            {
                WorkOrderNumber = "WO-10021",
                Vin = "1HGCM82633A004352",
                CustomerName = "Arun Kumar",
                CampaignId = "24V-102",
                Bay = "Bay 1",
                Technician = "Ravi",
                Time = "09:00 AM",
                Status = "Scheduled"
            },
            new RecallJob
            {
                WorkOrderNumber = "WO-10022",
                Vin = "1M8GDM9AXKP042788",
                CustomerName = "Priya Sharma",
                CampaignId = "24V-102",
                Bay = "Bay 2",
                Technician = "Ravi",
                Time = "11:00 AM",
                Status = "Checked In"
            },
            new RecallJob
            {
                WorkOrderNumber = "WO-10023",
                Vin = "JHMCM56557C404453",
                CustomerName = "Rahul Verma",
                CampaignId = "24V-102",
                Bay = "Bay 3",
                Technician = "Anita",
                Time = "02:00 PM",
                Status = "In Progress"
            }
        };

        Jobs = allJobs
            .Where(job =>
                BayFilter == "All Bays" ||
                job.Bay == BayFilter)
            .Where(job =>
                TechnicianFilter == "All Technicians" ||
                job.Technician == TechnicianFilter)
            .ToList();
    }

    public class RecallJob
    {
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string Vin { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CampaignId { get; set; } = string.Empty;
        public string Bay { get; set; } = string.Empty;
        public string Technician { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}