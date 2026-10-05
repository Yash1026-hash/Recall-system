using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecall.Shared.Models;

namespace VehicleRecallApp.Pages.Users;

[Authorize(Roles = "Manager")]
public class DetailsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DetailsModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public UserSummaryResponse? UserDetails { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        var response = await client.GetAsync($"/api/users/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = $"Unable to retrieve user details (HTTP {(int)response.StatusCode}).";
            return Page();
        }

        UserDetails = await response.Content.ReadFromJsonAsync<UserSummaryResponse>();
        if (UserDetails is null)
        {
            return NotFound();
        }

        return Page();
    }
}
