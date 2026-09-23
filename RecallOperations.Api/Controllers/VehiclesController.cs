using Microsoft.AspNetCore.Mvc;
using VehicleRecall.Shared.Models;

namespace RecallOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private static readonly List<Vehicle> Vehicles =
    [
        new Vehicle
        {
            Vin = "1HGCM82633A004352",
            Make = "Honda",
            Model = "Accord",
            Year = 2023,
            RecallStatus = "Open"
        },
        new Vehicle
        {
            Vin = "1FMCU0BZ5MUA12345",
            Make = "Ford",
            Model = "Escape",
            Year = 2021,
            RecallStatus = "Repair Scheduled"
        },
        new Vehicle
        {
            Vin = "JTDKN3DU5A0123456",
            Make = "Toyota",
            Model = "Prius",
            Year = 2020,
            RecallStatus = "Completed"
        },
        new Vehicle
        {
            Vin = "1G1JC5SH4C4123456",
            Make = "Chevrolet",
            Model = "Cruze",
            Year = 2019,
            RecallStatus = "Open"
        },
        new Vehicle
        {
            Vin = "WBA8E9G50JNU12345",
            Make = "BMW",
            Model = "3 Series",
            Year = 2018,
            RecallStatus = "No Open Recall"
        },
        new Vehicle
        {
            Vin = "5YFBURHE5FP123456",
            Make = "Toyota",
            Model = "Corolla",
            Year = 2022,
            RecallStatus = "Repair Scheduled"
        }
    ];

    [HttpGet]
    public ActionResult<IEnumerable<Vehicle>> GetAll()
    {
        return Ok(Vehicles);
    }

    [HttpGet("{vin}")]
    public ActionResult<Vehicle> GetByVin([FromRoute] string vin)
    {
        var vehicle = Vehicles.FirstOrDefault(v => v.Vin == vin);

        return vehicle is null ? NotFound() : Ok(vehicle);
    }
}
