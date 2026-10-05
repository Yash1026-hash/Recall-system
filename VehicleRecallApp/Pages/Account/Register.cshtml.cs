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
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "That username or email address is already in use.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var validationProblem =
                        await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

                    if (validationProblem?.Errors.Count > 0)
                    {
                        foreach (var (key, errors) in validationProblem.Errors)
                        {
                            var modelStateKey = key switch
                            {
                                "Email" or "Registration.Email" => "Registration.Email",
                                "Username" or "Registration.Username" => "Registration.Username",
                                "FullName" or "Registration.FullName" => "Registration.FullName",
                                "Password" or "Registration.Password" => "Registration.Password",
                                "ConfirmPassword" or "Registration.ConfirmPassword" =>
                                    "Registration.ConfirmPassword",
                                _ => string.Empty
                            };

                            foreach (var error in errors)
                            {
                                ModelState.AddModelError(modelStateKey, error);
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            "Registration details were rejected. Check the form and try again.");
                    }
                }
                else
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Registration could not be completed. Please try again.");
                }

                return Page();
            }

            RegistrationComplete = true;
            Registration = new RegisterRequest();
            return Page();
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Could not connect to the API server. Please try again.");
            return Page();
        }
        catch (TaskCanceledException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The API request timed out. Please try again.");
            return Page();
        }
    }
}