using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecallOperations.Api.Data;
using VehicleRecall.Shared.Models;

namespace RecallOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly UserDbContext _context;

    public VehiclesController(UserDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Vehicle>> GetAll()
    {
        var vehicles = _context.Vehicles
            .FromSqlRaw("EXEC dbo.usp_Vehicles_GetAll")
            .AsNoTracking()
            .ToList();

        return Ok(vehicles);
    }

    [HttpGet("{vin}")]
    public ActionResult<Vehicle> GetByVin([FromRoute] string vin)
    {
        var vehicle = _context.Vehicles
            .FromSqlInterpolated($"EXEC dbo.usp_Vehicles_GetByVin @Vin={vin}")
            .AsNoTracking()
            .ToList()
            .FirstOrDefault();

        return vehicle is null ? NotFound(new { message = "Vehicle not found." }) : Ok(vehicle);
    }

    [HttpPost]
    public ActionResult<Vehicle> CreateVehicle([FromBody] Vehicle vehicle)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        _context.Vehicles.Add(vehicle);
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetByVin), new { vin = vehicle.Vin }, vehicle);
    }

    [HttpPut("{vin}")]
    public ActionResult<Vehicle> UpdateVehicle([FromRoute] string vin, [FromBody] Vehicle vehicle)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (vehicle is null)
        {
            return BadRequest(new { message = "Vehicle payload is required." });
        }

        var vehicleToUpdate = _context.Vehicles.FirstOrDefault(existingVehicle => existingVehicle.Vin == vin);

        if (vehicleToUpdate is null)
        {
            return NotFound(new { message = "Vehicle not found." });
        }

        vehicleToUpdate.Make = vehicle.Make;
        vehicleToUpdate.Model = vehicle.Model;
        vehicleToUpdate.Year = vehicle.Year;
        vehicleToUpdate.RecallStatus = vehicle.RecallStatus;

        _context.SaveChanges();

        return Ok(vehicleToUpdate);
    }

    [HttpDelete("{vin}")]
    public IActionResult DeleteVehicle([FromRoute] string vin)
    {
        var vehicleToDelete = _context.Vehicles.FirstOrDefault(v => v.Vin == vin);

        if (vehicleToDelete is null)
        {
            return NotFound(new { message = "Vehicle not found." });
        }

        _context.Vehicles.Remove(vehicleToDelete);
        _context.SaveChanges();

        return NoContent();
    }
}