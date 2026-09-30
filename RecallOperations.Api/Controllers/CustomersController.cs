using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecallOperations.Api.Services;
using VehicleRecall.Shared.Models;

namespace RecallOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly UserStore _userStore;

    public CustomersController(UserStore userStore)
    {
        _userStore = userStore;
    }

    [HttpPost("roster")]
    [Authorize(Policy = "ManagerOnly")]
    public ActionResult<CustomerRosterImportResponse> ImportRoster(
        [FromBody] List<CustomerRosterEntryRequest> customers)
    {
        if (customers.Count == 0)
        {
            return BadRequest(new { message = "At least one valid customer is required." });
        }

        var result = _userStore.ImportCustomerRoster(customers);
        return Ok(new CustomerRosterImportResponse(result.Added, result.Updated));
    }
}