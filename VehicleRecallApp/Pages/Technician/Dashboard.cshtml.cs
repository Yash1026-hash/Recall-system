using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VehicleRecallApp.Pages.Technician;

public class DashboardModel : PageModel
{
    public int JobsToday { get; private set; }
    public int CheckedInJobs { get; private set; }
    public int InProgressJobs { get; private set; }
    public int ReadyForPickupJobs { get; private set; }

    public void OnGet()
    {
        // Temporary workshop dashboard data.
        // Later, retrieve this from appointments and work orders in the database.
        JobsToday = 14;
        CheckedInJobs = 3;
        InProgressJobs = 5;
        ReadyForPickupJobs = 2;
    }
}