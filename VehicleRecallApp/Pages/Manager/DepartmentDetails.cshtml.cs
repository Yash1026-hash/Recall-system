using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecall.Shared.Models;

namespace VehicleRecallApp.Pages.Manager;

public class DepartmentDetailsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DepartmentDetailsModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public DepartmentResponse? Department { get; private set; }
    public List<UserSummaryResponse> Users { get; private set; } = new();
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var response = await _httpClientFactory.CreateClient("ApiClient")
            .GetAsync($"/api/departments/{id}");
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

        if (Department.UserCount > 0)
        {
            var usersResponse = await _httpClientFactory.CreateClient("ApiClient")
                .GetAsync($"/api/users?departmentId={id}&page=1&pageSize=100");
            if (!usersResponse.IsSuccessStatusCode)
            {
                ErrorMessage = await ReadErrorAsync(usersResponse);
                return Page();
            }

            var users = await usersResponse.Content.ReadFromJsonAsync<List<UserSummaryResponse>>();
            if (users is null)
            {
                ErrorMessage = "The API returned an empty user list response.";
                return Page();
            }

            Users = users;
        }

        return Page();
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(content)
            ? $"Request failed with status {(int)response.StatusCode}."
            : $"Request failed with status {(int)response.StatusCode}: {content}";
    }
}
