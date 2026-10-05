using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecall.Shared.Models;

namespace VehicleRecallApp.Pages.Manager;

public class EditDepartmentModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EditDepartmentModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public DepartmentInput Input { get; set; } = new();

    public DepartmentResponse? Department { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (Id <= 0)
        {
            return NotFound();
        }

        var response = await _httpClientFactory.CreateClient("ApiClient")
            .GetAsync($"/api/departments/{Id}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = await ReadErrorAsync(response);
            return Page();
        }

        Department = await response.Content.ReadFromJsonAsync<DepartmentResponse>();
        if (Department is null)
        {
            return NotFound();
        }

        Input = new DepartmentInput { Name = Department.Name, Description = Department.Description };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Id <= 0)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await LoadDepartmentAsync();
            return Page();
        }

        var client = _httpClientFactory.CreateClient("ApiClient");
        var response = await client.PutAsJsonAsync($"/api/departments/{Id}", new UpdateDepartmentRequest
        {
            Name = Input.Name,
            Description = Input.Description
        });
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = await ReadErrorAsync(response);
            await LoadDepartmentAsync();
            return Page();
        }

        TempData["SuccessMessage"] = "Department updated.";
        return RedirectToPage("/Manager/DepartmentDetails", new { id = Id });
    }

    private async Task LoadDepartmentAsync()
    {
        var response = await _httpClientFactory.CreateClient("ApiClient")
            .GetAsync($"/api/departments/{Id}");
        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage ??= await ReadErrorAsync(response);
            return;
        }

        Department = await response.Content.ReadFromJsonAsync<DepartmentResponse>();
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
