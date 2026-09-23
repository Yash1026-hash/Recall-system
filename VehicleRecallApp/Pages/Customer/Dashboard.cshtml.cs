using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VehicleRecallApp.Pages.Customer;

public class DashboardModel : PageModel
{
    public string OwnerName { get; private set; } = string.Empty;

    public void OnGet()
    {
        OwnerName = "Vehicle Owner";
    }
}