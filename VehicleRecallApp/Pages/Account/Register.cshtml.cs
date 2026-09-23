using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecall.Shared.Models;

namespace VehicleRecallApp.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RegisterModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public RegisterRequest Registration { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public bool RegistrationComplete { get; private set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client = _httpClientFactory.CreateClient("ApiClient");

        try
        {
            var response = await client.PostAsJsonAsync(
                "/api/users/register",
                Registration);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Conflict =>
                        "That username or email address is already in use.",
                    _ =>
                        "Registration could not be completed."
                };

                return Page();
            }

            RegistrationComplete = true;
            Registration = new RegisterRequest();
            return Page();
        }
        catch
        {
            ErrorMessage = "Could not connect to the API server.";
            return Page();
        }
    }
}