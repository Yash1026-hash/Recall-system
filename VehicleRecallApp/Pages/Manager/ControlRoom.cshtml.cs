using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecall.Shared.Models;

namespace VehicleRecallApp.Pages.Manager;

public class ControlRoomModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public List<UserSummaryResponse> Users { get; private set; } = new();
    public List<DepartmentResponse> Departments { get; private set; } = new();
    public List<string> AvailableRoles { get; private set; } = new();
    public string? ErrorMessage { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Role { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? DepartmentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActive { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? RegistrationStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty]
    public StaffUserRequest StaffInput { get; set; } = new();

    [BindProperty]
    public RoleChangeInput RoleChange { get; set; } = new();

    [BindProperty]
    public AccountStatusInput AccountStatus { get; set; } = new();

    public ControlRoomModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnGetAsync()
    {
        await LoadLookupsAsync();
        await LoadUsersAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        RemoveModelStatePrefix(nameof(RoleChange));
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync();
            await LoadUsersAsync();
            return Page();
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        var response = await client.PostAsJsonAsync("/api/users/staff", StaffInput);
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await ReadErrorAsync(response);
            return RedirectToPage(new { Search, Role, DepartmentId, IsActive, RegistrationStatus, PageNumber });
        }

        TempData["SuccessMessage"] = $"{StaffInput.Role} account created.";
        return RedirectToPage(new { Search, Role, DepartmentId, IsActive, RegistrationStatus, PageNumber });
    }

    public async Task<IActionResult> OnPostRolesAsync()
    {
        RemoveModelStatePrefix(nameof(StaffInput));
        RemoveModelStatePrefix(nameof(AccountStatus));
        if (RoleChange.UserId < 1 ||
            RoleChange.Roles is null ||
            RoleChange.Roles.Count == 0 ||
            RoleChange.Roles.Any(role => role is not ("Manager" or "Technician" or "Customer")))
        {
            TempData["ErrorMessage"] = "Choose at least one valid role.";
            return RedirectToPage(new { Search, Role, DepartmentId, IsActive, RegistrationStatus, PageNumber });
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        var response = await client.PutAsJsonAsync(
            $"/api/users/{RoleChange.UserId}/roles",
            new UpdateUserRolesRequest { Roles = RoleChange.Roles });
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await ReadErrorAsync(response);
            return RedirectToPage(new { Search, Role, DepartmentId, IsActive, RegistrationStatus, PageNumber });
        }

        TempData["SuccessMessage"] = "User roles updated.";
        return RedirectToPage(new { Search, Role, DepartmentId, IsActive, RegistrationStatus, PageNumber });
    }

    public async Task<IActionResult> OnPostStatusAsync()
    {
        RemoveModelStatePrefix(nameof(StaffInput));
        RemoveModelStatePrefix(nameof(RoleChange));
        if (AccountStatus.UserId < 1)
        {
            TempData["ErrorMessage"] = "Choose a valid user.";
            return RedirectToPage(new { Search, Role, DepartmentId, IsActive, RegistrationStatus, PageNumber });
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        var response = await client.PatchAsJsonAsync(
            $"/api/users/{AccountStatus.UserId}/status",
            new UpdateUserStatusRequest(AccountStatus.IsActive));
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await ReadErrorAsync(response);
        }
        else
        {
            TempData["SuccessMessage"] = AccountStatus.IsActive
                ? "User account activated."
                : "User account deactivated.";
        }

        return RedirectToPage(new { Search, Role, DepartmentId, IsActive, RegistrationStatus, PageNumber });
    }

    private async Task LoadUsersAsync()
    {
        PageNumber = Math.Max(PageNumber, 1);
        var query = new List<string> { $"page={PageNumber}", "pageSize=100" };
        if (!string.IsNullOrWhiteSpace(Search))
        {
            query.Add($"search={Uri.EscapeDataString(Search)}");
        }

        if (!string.IsNullOrWhiteSpace(Role))
        {
            query.Add($"role={Uri.EscapeDataString(Role)}");
        }

        if (DepartmentId.HasValue)
        {
            query.Add($"departmentId={DepartmentId.Value}");
        }

        if (IsActive.HasValue)
        {
            query.Add($"isActive={IsActive.Value.ToString().ToLowerInvariant()}");
        }

        if (!string.IsNullOrWhiteSpace(RegistrationStatus))
        {
            query.Add($"registrationStatus={Uri.EscapeDataString(RegistrationStatus)}");
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        var response = await client.GetAsync("/api/users?" + string.Join("&", query));
        if (response.IsSuccessStatusCode)
        {
            var users = await response.Content.ReadFromJsonAsync<List<UserSummaryResponse>>();
            if (users is null)
            {
                ErrorMessage = "The API returned an empty user list response.";
                return;
            }

            Users = users;
            return;
        }

        ErrorMessage = await ReadErrorAsync(response);
    }

    private async Task LoadLookupsAsync()
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        var departmentsResponse = await client.GetAsync("/api/departments");
        if (departmentsResponse.IsSuccessStatusCode)
        {
            var departments = await departmentsResponse.Content.ReadFromJsonAsync<List<DepartmentResponse>>();
            if (departments is null)
            {
                ErrorMessage = "The API returned an empty department list response.";
            }
            else
            {
                Departments = departments;
            }
        }
        else
        {
            ErrorMessage = await ReadErrorAsync(departmentsResponse);
        }

        var rolesResponse = await client.GetAsync("/api/users/roles");
        if (rolesResponse.IsSuccessStatusCode)
        {
            var roles = await rolesResponse.Content.ReadFromJsonAsync<List<string>>();
            if (roles is null)
            {
                ErrorMessage = "The API returned an empty role list response.";
            }
            else
            {
                AvailableRoles = roles;
            }
        }
        else
        {
            ErrorMessage = await ReadErrorAsync(rolesResponse);
        }
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
        public List<string> Roles { get; set; } = new();
    }

    public sealed class AccountStatusInput
    {
        public int UserId { get; set; }
        public bool IsActive { get; set; }
    }
}
