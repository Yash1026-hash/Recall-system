using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecall.Shared.Models;

namespace VehicleRecallApp.Pages.Users;

[Authorize(Roles = "Manager")]
public class DeleteModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DeleteModel(IHttpClientFactory httpClientFactory)
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

        var response = await _httpClientFactory.CreateClient("ApiClient")
            .GetAsync($"/api/users/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = await ReadErrorAsync(response);
            return Page();
        }

        UserDetails = await response.Content.ReadFromJsonAsync<UserSummaryResponse>();
        return UserDetails is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        var response = await client.DeleteAsync($"/api/users/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            TempData["ErrorMessage"] = "User not found; it may already have been deleted.";
            return RedirectToPage("/Manager/ControlRoom");
        }

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = await ReadErrorAsync(response);
            await LoadUserDetailsAsync(client, id);
            return Page();
        }

        TempData["SuccessMessage"] = "User account deleted.";
        return RedirectToPage("/Manager/ControlRoom");
    }

    private async Task LoadUserDetailsAsync(HttpClient client, int id)
    {
        var response = await client.GetAsync($"/api/users/{id}");
        if (response.IsSuccessStatusCode)
        {
            UserDetails = await response.Content.ReadFromJsonAsync<UserSummaryResponse>();
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(content)
            ? $"Unable to delete user (HTTP {(int)response.StatusCode})."
            : $"Unable to delete user (HTTP {(int)response.StatusCode}): {content}";
    }
}
