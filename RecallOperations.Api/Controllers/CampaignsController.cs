using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecallOperations.Api.Data;
using VehicleRecall.Shared.Models;

namespace RecallOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CampaignsController : ControllerBase
{
    private readonly UserDbContext _context;

    public CampaignsController(UserDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<RecallCampaign>>> GetAll()
    {
        var campaigns = await _context.Campaigns
            .Include(c => c.CampaignVehicles)
            .OrderByDescending(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        foreach (var c in campaigns)
        {
            c.AffectedVins = c.CampaignVehicles.Count;
            // Build lightweight user status list for counting Booked & Repaired
            c.Users = c.CampaignVehicles.Select(cv => new CampaignUserStatus
            {
                Vin = cv.Vin,
                AppointmentBooked = cv.AppointmentBooked,
                RepairCompleted = cv.RepairCompleted
            }).ToList();
        }

        return Ok(campaigns);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RecallCampaign>> GetById([FromRoute] string id)
    {
        var campaign = await _context.Campaigns
            .Include(c => c.CampaignVehicles)
                .ThenInclude(cv => cv.Vehicle)
                    .ThenInclude(v => v!.CustomerVehicles)
                        .ThenInclude(cv => cv.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.NhtsaId == id);

        if (campaign is null)
        {
            return NotFound(new { message = "Campaign not found." });
        }

        campaign.Users = campaign.CampaignVehicles.Select(cv =>
        {
            var customer = cv.Vehicle?.CustomerVehicles.FirstOrDefault()?.Customer;
            return new CampaignUserStatus
            {
                CustomerName = customer?.FullName ?? "Unknown",
                Email = customer?.Email ?? "N/A",
                Vin = cv.Vin,
                Address = customer?.Address,
                City = customer?.City,
                State = customer?.State,
                PostalCode = customer?.PostalCode,
                AppointmentBooked = cv.AppointmentBooked,
                RepairCompleted = cv.RepairCompleted
            };
        }).ToList();

        campaign.AffectedVins = campaign.CampaignVehicles.Count;
        return Ok(campaign);
    }

    [HttpPost]
    public async Task<ActionResult<RecallCampaign>> Create([FromBody] RecallCampaign campaign)
    {
        if (string.IsNullOrWhiteSpace(campaign.NhtsaId))
        {
            return BadRequest(new { message = "Campaign NHTSA ID is required." });
        }

        var existingCampaign = await _context.Campaigns.FindAsync(campaign.NhtsaId);
        if (existingCampaign is null)
        {
            existingCampaign = new RecallCampaign
            {
                NhtsaId = campaign.NhtsaId,
                Description = campaign.Description,
                AffectedComponent = campaign.AffectedComponent,
                RemedyInstructions = campaign.RemedyInstructions,
                Severity = campaign.Severity,
                Status = string.IsNullOrWhiteSpace(campaign.Status) ? "Active" : campaign.Status,
                CreatedAt = DateTime.UtcNow,
                ImportedRecords = campaign.ImportedRecords,
                SuccessfulImports = campaign.SuccessfulImports,
                FailedImports = campaign.FailedImports,
                AffectedVins = campaign.Users.Count
            };
            _context.Campaigns.Add(existingCampaign);
        }
        else
        {
            existingCampaign.Description = campaign.Description;
            existingCampaign.AffectedComponent = campaign.AffectedComponent;
            existingCampaign.RemedyInstructions = campaign.RemedyInstructions;
            existingCampaign.Severity = campaign.Severity;
            existingCampaign.Status = campaign.Status;
            existingCampaign.ImportedRecords = campaign.ImportedRecords;
            existingCampaign.SuccessfulImports = campaign.SuccessfulImports;
            existingCampaign.FailedImports = campaign.FailedImports;
            existingCampaign.AffectedVins = Math.Max(existingCampaign.AffectedVins, campaign.Users.Count);
        }

        await _context.SaveChangesAsync();

        // Process users/vehicles
        foreach (var user in campaign.Users)
        {
            if (string.IsNullOrWhiteSpace(user.Vin))
            {
                continue;
            }

            // 1. Ensure Customer exists
            RecallCustomer? customer = null;
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == user.Email.Trim());
                if (customer is null)
                {
                    customer = new RecallCustomer
                    {
                        FullName = string.IsNullOrWhiteSpace(user.CustomerName) ? "Unknown" : user.CustomerName.Trim(),
                        Email = user.Email.Trim(),
                        Address = user.Address,
                        City = user.City,
                        State = user.State,
                        PostalCode = user.PostalCode
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(user.CustomerName)) customer.FullName = user.CustomerName;
                    if (!string.IsNullOrWhiteSpace(user.Address)) customer.Address = user.Address;
                    if (!string.IsNullOrWhiteSpace(user.City)) customer.City = user.City;
                    if (!string.IsNullOrWhiteSpace(user.State)) customer.State = user.State;
                    if (!string.IsNullOrWhiteSpace(user.PostalCode)) customer.PostalCode = user.PostalCode;
                }
            }
            // 2. Ensure Vehicle exists in KS_Vehicles
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Vin == user.Vin.Trim());
            if (vehicle is null)
            {
                vehicle = new Vehicle
                {
                    Vin = user.Vin.Trim(),
                    Make = "Standard",
                    Model = "Vehicle",
                    Year = 2021,
                    RecallStatus = "Open"
                };
                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();
            }
            else
            {
                vehicle.RecallStatus = "Open";
            }

            // 3. Link Customer to Vehicle if customer exists
            if (customer is not null)
            {
                var existingLink = await _context.CustomerVehicles
                    .FirstOrDefaultAsync(cv => cv.CustomerId == customer.CustomerId && cv.Vin == vehicle.Vin);
                if (existingLink is null)
                {
                    _context.CustomerVehicles.Add(new RecallCustomerVehicle
                    {
                        CustomerId = customer.CustomerId,
                        Vin = vehicle.Vin
                    });
                }
            }

            // 4. Link Campaign to Vehicle
            var existingCampaignVehicle = await _context.CampaignVehicles
                .FirstOrDefaultAsync(cv => cv.CampaignId == campaign.NhtsaId && cv.Vin == vehicle.Vin);
            if (existingCampaignVehicle is null)
            {
                _context.CampaignVehicles.Add(new CampaignVehicle
                {
                    CampaignId = campaign.NhtsaId,
                    Vin = vehicle.Vin,
                    AppointmentBooked = user.AppointmentBooked,
                    RepairCompleted = user.RepairCompleted
                });
            }
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = campaign.NhtsaId }, campaign);
    }
}
