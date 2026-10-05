using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecallOperations.Api.Data;
using VehicleRecall.Shared.Models;

namespace RecallOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly UserDbContext _context;

    public ReportsController(UserDbContext context)
    {
        _context = context;
    }

    [HttpGet("aggregated")]
    public ActionResult<AggregatedReportResponse> GetAggregatedReports()
    {
        var response = new AggregatedReportResponse();

        try
        {
            response.VehiclesByStatus = _context.Database
                .SqlQuery<VehicleRecallStatusSummary>($"EXEC dbo.usp_TotalVehiclesByRecallStatus")
                .ToList();
        }
        catch
        {
            // Allows partial report if procedure is not yet created
        }

        try
        {
            response.CustomersByCityState = _context.Database
                .SqlQuery<CustomerCityStateSummary>($"EXEC dbo.usp_CustomersByCityAndState")
                .ToList();
        }
        catch
        {
            // Allows partial report if procedure is not yet created
        }

        try
        {
            response.UsersByRole = _context.Database
                .SqlQuery<UserRoleSummary>($"EXEC dbo.usp_UsersByRole")
                .ToList();
        }
        catch
        {
            // Allows partial report if procedure is not yet created
        }

        try
        {
            response.UsersByRegistrationStatus = _context.Database
                .SqlQuery<UserRegistrationSummary>($"EXEC dbo.usp_UsersByRegistrationStatus")
                .ToList();
        }
        catch
        {
            // Allows partial report if procedure is not yet created
        }

        return Ok(response);
    }
}
