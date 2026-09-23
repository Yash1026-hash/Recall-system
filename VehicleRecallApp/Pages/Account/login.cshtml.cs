using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecall.Shared.Models;

namespace VehicleRecallApp.Pages.Account;

public class LoginModel : PageModel
{
    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required.";
            return Page();
        }

        var client = HttpContext.RequestServices
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient("ApiClient");

        var loginRequest = new LoginRequest
        {
            Username = Username.Trim(),
            Password = Password
        };

        try
        {
            var response = await client.PostAsJsonAsync("/api/users/login", loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Invalid username or password.";
                return Page();
            }

            var user = await response.Content.ReadFromJsonAsync<UserAccount>();

            if (user is null)
            {
                ErrorMessage = "Login failed.";
                return Page();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            return RedirectToPage($"/{user.Role}/Dashboard");
        }
        catch
        {
            ErrorMessage = "Could not connect to the API server.";
            return Page();
        }
    }
    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToPage("/Index");
    }
}