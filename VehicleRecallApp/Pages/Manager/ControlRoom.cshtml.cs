using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecall.Shared.Models;

namespace VehicleRecallApp.Pages.Manager;

public class ControlRoomModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public List<UserSummaryResponse> Users { get; private set; } = new();
    public string? ErrorMessage { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Role { get; set; }

    [BindProperty]
    public StaffUserRequest StaffInput { get; set; } = new();

    [BindProperty]
    public RoleChangeInput RoleChange { get; set; } = new();

    public ControlRoomModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnGetAsync()
    {
        await LoadUsersAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        RemoveModelStatePrefix(nameof(RoleChange));
        if (!ModelState.IsValid)
        {
            await LoadUsersAsync();
            return Page();
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        var response = await client.PostAsJsonAsync("/api/users/staff", StaffInput);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await ReadErrorAsync(response);
            return RedirectToPage(new { Search, Role });
        }

        TempData["SuccessMessage"] = $"{StaffInput.Role} account created.";
        return RedirectToPage(new { Search, Role });
    }

    public async Task<IActionResult> OnPostRoleAsync()
    {
        RemoveModelStatePrefix(nameof(StaffInput));
        if (RoleChange.UserId < 1 || RoleChange.Role is not ("Manager" or "Technician" or "Customer"))
        {
            TempData["ErrorMessage"] = "Choose a valid user and role.";
            return RedirectToPage(new { Search, Role });
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        var response = await client.PatchAsJsonAsync(
            $"/api/users/{RoleChange.UserId}/role",
            new UpdateUserRoleRequest { Role = RoleChange.Role });
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await ReadErrorAsync(response);
            return RedirectToPage(new { Search, Role });
        }

        TempData["SuccessMessage"] = "User role updated.";
        return RedirectToPage(new { Search, Role });
    }

    private async Task LoadUsersAsync()
    {
        var query = new List<string> { "registrationStatus=Registered", "page=1", "pageSize=100" };
        if (!string.IsNullOrWhiteSpace(Search))
        {
            query.Add($"search={Uri.EscapeDataString(Search)}");
        }

        if (!string.IsNullOrWhiteSpace(Role))
        {
            query.Add($"role={Uri.EscapeDataString(Role)}");
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        var response = await client.GetAsync("/api/users?" + string.Join("&", query));
        if (response.IsSuccessStatusCode)
        {
            Users = await response.Content.ReadFromJsonAsync<List<UserSummaryResponse>>() ?? new();
            return;
        }

        ErrorMessage = await ReadErrorAsync(response);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(content)
            ? $"Request failed with status {(int)response.StatusCode}."
            : $"Request failed with status {(int)response.StatusCode}: {content}";
    }

    private void RemoveModelStatePrefix(string prefix)
    {
        foreach (var key in ModelState.Keys
                     .Where(key => key.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
                                   key.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase))
                     .ToList())
        {
            ModelState.Remove(key);
        }
    }

    public sealed class RoleChangeInput
    {
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
