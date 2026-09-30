using Microsoft.VisualBasic.FileIO;
using System.ComponentModel.DataAnnotations;
using VehicleRecallApp.Models;

namespace VehicleRecallApp.Services;

public sealed class CampaignCsvImporter
{
    public string? Import(
        RecallCampaign campaign,
        Stream csvStream,
        out IReadOnlyList<CampaignUserStatus> importedCustomers)
    {
        importedCustomers = Array.Empty<CampaignUserStatus>();
        var errors = new List<CampaignImportError>();
        var parsedCustomers = new Dictionary<string, CampaignUserStatus>(StringComparer.OrdinalIgnoreCase);
        var totalRecords = 0;
        var successfulImports = 0;

        using (var parser = new TextFieldParser(csvStream))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;
            parser.TrimWhiteSpace = false;

            var headers = parser.ReadFields();
            if (headers is null)
            {
                return "The CSV file is empty.";
            }

            var headerMap = headers
                .Select((header, index) => new { Name = header.Trim(), Index = index })
                .GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().Index, StringComparer.OrdinalIgnoreCase);

            if (!headerMap.TryGetValue("FullName", out var nameIndex) ||
                !headerMap.TryGetValue("Email", out var emailIndex) ||
                !headerMap.TryGetValue("VIN", out var vinIndex))
            {
                return "CSV headers must include FullName, Email, and VIN.";
            }

            var addressIndex = GetHeaderIndex(headerMap, "Address");
            var cityIndex = GetHeaderIndex(headerMap, "City");
            var stateIndex = GetHeaderIndex(headerMap, "State");
            var postalCodeIndex = GetHeaderIndex(headerMap, "PostalCode");

            var rowNumber = 1;
            while (!parser.EndOfData)
            {
                rowNumber++;
                string[]? fields;
                try
                {
                    fields = parser.ReadFields();
                }
                catch (MalformedLineException)
                {
                    totalRecords++;
                    errors.Add(new CampaignImportError { RowNumber = rowNumber, Reason = "Malformed CSV row." });
                    continue;
                }

                if (fields is null)
                {
                    continue;
                }

                totalRecords++;
                if (fields.Length <= Math.Max(nameIndex, Math.Max(emailIndex, vinIndex)))
                {
                    errors.Add(new CampaignImportError { RowNumber = rowNumber, Reason = "Row is missing required fields." });
                    continue;
                }

                var fullName = fields[nameIndex].Trim();
                var email = fields[emailIndex].Trim();
                var vin = fields[vinIndex].Trim().ToUpperInvariant();
                var address = GetOptionalField(fields, addressIndex);
                var city = GetOptionalField(fields, cityIndex);
                var state = GetOptionalField(fields, stateIndex);
                var postalCode = GetOptionalField(fields, postalCodeIndex);
                var validationError = GetValidationError(fullName, email, vin, address, city, state, postalCode);
                if (validationError is not null)
                {
                    errors.Add(new CampaignImportError { RowNumber = rowNumber, Vin = vin, Reason = validationError });
                    continue;
                }

                parsedCustomers[vin] = new CampaignUserStatus
                {
                    CustomerName = fullName,
                    Email = email,
                    Vin = vin,
                    Address = address,
                    City = city,
                    State = state,
                    PostalCode = postalCode
                };
                successfulImports++;
            }
        }

        importedCustomers = parsedCustomers.Values.ToList();
        foreach (var importedUser in importedCustomers)
        {
            var existingUser = campaign.Users.FirstOrDefault(user =>
                string.Equals(user.Vin, importedUser.Vin, StringComparison.OrdinalIgnoreCase));
            if (existingUser is null)
            {
                campaign.Users.Add(importedUser);
            }
            else
            {
                existingUser.CustomerName = importedUser.CustomerName;
                existingUser.Email = importedUser.Email;
                existingUser.Address = importedUser.Address ?? existingUser.Address;
                existingUser.City = importedUser.City ?? existingUser.City;
                existingUser.State = importedUser.State ?? existingUser.State;
                existingUser.PostalCode = importedUser.PostalCode ?? existingUser.PostalCode;
            }
        }

        campaign.ImportedRecords = totalRecords;
        campaign.SuccessfulImports = successfulImports;
        campaign.FailedImports = errors.Count;
        campaign.ImportErrors = errors;
        campaign.AffectedVins = Math.Max(campaign.AffectedVins, campaign.Users.Count);
        return null;
    }

    private static int? GetHeaderIndex(IReadOnlyDictionary<string, int> headers, string name) =>
        headers.TryGetValue(name, out var index) ? index : null;

    private static string? GetOptionalField(string[] fields, int? index)
    {
        if (index is null || index.Value >= fields.Length)
        {
            return null;
        }

        var value = fields[index.Value].Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? GetValidationError(
        string fullName,
        string email,
        string vin,
        string? address,
        string? city,
        string? state,
        string? postalCode)
    {
        if (fullName.Length is < 2 or > 100 || string.IsNullOrWhiteSpace(email))
        {
            return "Customer name must be 2-100 characters, and email is required.";
        }

        if (vin.Length != 17)
        {
            return "VIN must contain exactly 17 characters.";
        }

        if (vin.Any(character => !char.IsLetterOrDigit(character) || "IOQ".Contains(character)))
        {
            return "VIN must contain only letters and numbers and cannot include I, O, or Q.";
        }

        if (email.Length > 254 || !new EmailAddressAttribute().IsValid(email))
        {
            return "Customer email is not valid.";
        }

        if (address?.Length > 250 || city?.Length > 50 || state?.Length > 50 || postalCode?.Length > 10)
        {
            return "Address, city, state, or postal code exceeds the database column length.";
        }

        return null;
    }
}