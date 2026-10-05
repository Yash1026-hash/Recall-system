using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecall.Shared.Models;

namespace VehicleRecallApp.Pages.Users;

[Authorize(Roles = "Manager")]
public class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EditModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public UserEditInput Input { get; set; } = new();

    public UserSummaryResponse? UserDetails { get; private set; }
    public List<DepartmentResponse> Departments { get; private set; } = new();
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (Id <= 0)
        {
            return NotFound();
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        var response = await client.GetAsync($"/api/users/{Id}");
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
        if (UserDetails is null)
        {
            return NotFound();
        }

        Input = new UserEditInput
        {
            Username = UserDetails.Username,
            FullName = UserDetails.FullName,
            Email = UserDetails.Email,
            DepartmentId = UserDetails.DepartmentId
        };
        await LoadDepartmentsAsync(client);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Id <= 0)
        {
            return NotFound();
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        if (!ModelState.IsValid)
        {
            await LoadPageDataAsync(client);
            return Page();
        }

        var response = await client.PatchAsJsonAsync($"/api/users/{Id}/profile", new UpdateUserRequest
        {
            Username = Input.Username,
            FullName = Input.FullName,
            Email = Input.Email,
            DepartmentId = Input.DepartmentId
        });
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToPage("/Manager/ControlRoom");
        }

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = await ReadErrorAsync(response);
            await LoadPageDataAsync(client);
            return Page();
        }

        TempData["SuccessMessage"] = "User details updated.";
        return RedirectToPage("/Users/Details", new { id = Id });
    }

    private async Task LoadPageDataAsync(HttpClient client)
    {
        var response = await client.GetAsync($"/api/users/{Id}");
        if (response.IsSuccessStatusCode)
        {
            UserDetails = await response.Content.ReadFromJsonAsync<UserSummaryResponse>();
        }

        await LoadDepartmentsAsync(client);
    }

    private async Task LoadDepartmentsAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/departments");
        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage ??= await ReadErrorAsync(response);
            return;
        }

        var departments = await response.Content.ReadFromJsonAsync<List<DepartmentResponse>>();
        if (departments is null)
        {
            ErrorMessage ??= "The API returned an empty department list response.";
            return;
        }

        Departments = departments;
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(content)
            ? $"Request failed with status {(int)response.StatusCode}."
            : $"Request failed with status {(int)response.StatusCode}: {content}";
    }

    public sealed class UserEditInput
    {
        [Required, StringLength(30, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_.]+$")]
        public string Username { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Choose a department.")]
        public int DepartmentId { get; set; }
    }
}
