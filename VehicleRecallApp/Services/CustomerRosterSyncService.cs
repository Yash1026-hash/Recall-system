using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using VehicleRecall.Shared.Models;
using VehicleRecallApp.Models;

namespace VehicleRecallApp.Services;

public sealed class CustomerRosterSyncService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CustomerRosterSyncService> _logger;

    public CustomerRosterSyncService(
        IHttpClientFactory httpClientFactory,
        ILogger<CustomerRosterSyncService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string?> SyncAsync(IEnumerable<CampaignUserStatus> importedCustomers)
    {
        var rosterEntries = importedCustomers
            .GroupBy(customer => customer.Email, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Select(customer => new CustomerRosterEntryRequest
            {
                FullName = customer.CustomerName,
                Email = customer.Email,
                Address = customer.Address,
                City = customer.City,
                State = customer.State,
                PostalCode = customer.PostalCode
            })
            .ToList();

        if (rosterEntries.Count == 0)
        {
            return null;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsJsonAsync("/api/customers/roster", rosterEntries);
            if (response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "Customer roster sync failed with HTTP {StatusCode}: {ResponseBody}",
                (int)response.StatusCode,
                responseBody);

            return response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Your manager session expired. Sign out, sign back in, then upload the CSV again.",
                System.Net.HttpStatusCode.Forbidden => "Only a manager can import customer emails. Sign in with a manager account and retry.",
                System.Net.HttpStatusCode.BadRequest => GetValidationError(responseBody),
                _ => $"The API could not save customer emails (HTTP {(int)response.StatusCode}). Check the API log and retry."
            };
        }
        catch (HttpRequestException)
        {
            return "The API could not save customer emails from this CSV for registration. Check that the API is running and retry the CSV.";
        }
    }

    private static string GetValidationError(string responseBody)
    {
        var problem = System.Text.Json.JsonSerializer.Deserialize<ValidationProblemDetails>(
            responseBody,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        var errors = problem?.Errors.SelectMany(error => error.Value).ToList();
        return errors is { Count: > 0 }
            ? $"The API rejected CSV data: {string.Join(" ", errors)}"
            : "The API rejected the CSV data. Check that names, emails, and optional address fields meet their length limits.";
    }
}