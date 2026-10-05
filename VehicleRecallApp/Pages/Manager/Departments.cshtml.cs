using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecall.Shared.Models;

namespace VehicleRecallApp.Pages.Manager;

public class DepartmentsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DepartmentsModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<DepartmentResponse> Departments { get; private set; } = new();
    public string? ErrorMessage { get; private set; }

    [BindProperty]
    public DepartmentInput Input { get; set; } = new();

    public async Task OnGetAsync() => await LoadDepartmentsAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync();
            return Page();
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        var response = await client.PostAsJsonAsync("/api/departments", new CreateDepartmentRequest
        {
            Name = Input.Name,
            Description = Input.Description
        });
        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = await ReadErrorAsync(response);
            await LoadDepartmentsAsync();
            return Page();
        }

        TempData["SuccessMessage"] = "Department created.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var response = await _httpClientFactory.CreateClient("ApiClient")
            .DeleteAsync($"/api/departments/{id}");
        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = await ReadErrorAsync(response);
        }
        else
        {
            TempData["SuccessMessage"] = "Department deleted.";
        }

        return RedirectToPage();
    }

    private async Task LoadDepartmentsAsync()
    {
        var response = await _httpClientFactory.CreateClient("ApiClient")
            .GetAsync("/api/departments");
        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = await ReadErrorAsync(response);
            return;
        }

        var departments = await response.Content.ReadFromJsonAsync<List<DepartmentResponse>>();
        if (departments is null)
        {
            ErrorMessage = "The API returned an empty department list response.";
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

    public sealed class DepartmentInput
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}
