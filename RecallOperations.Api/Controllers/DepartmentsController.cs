using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecallOperations.Api.Services;
using VehicleRecall.Shared.Models;

namespace RecallOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ManagerOnly")]
public class DepartmentsController : ControllerBase
{
    private readonly UserStore _userStore;

    public DepartmentsController(UserStore userStore)
    {
        _userStore = userStore;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<DepartmentResponse>> GetDepartments() =>
        Ok(_userStore.GetDepartmentSummaries());

    [HttpGet("{id:int}")]
    public ActionResult<DepartmentResponse> GetDepartment([FromRoute] int id)
    {
        var department = _userStore.GetDepartmentById(id);
        return department is null
            ? NotFound(new { message = "Department not found." })
            : Ok(ToResponse(department));
    }

    [HttpPost]
    public ActionResult<DepartmentResponse> CreateDepartment([FromBody] CreateDepartmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var department = _userStore.CreateDepartment(request);
            return CreatedAtAction(
                nameof(GetDepartment),
                new { id = department.DepartmentId },
                ToResponse(department));
        }
        catch (InvalidOperationException ex) when (ex.Message == "DepartmentAlreadyExists")
        {
            return Conflict(new { message = "A department with that name already exists." });
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return Conflict(new { message = "A department with that name already exists." });
        }
    }

    [HttpPut("{id:int}")]
    public ActionResult<DepartmentResponse> UpdateDepartment(
        [FromRoute] int id,
        [FromBody] UpdateDepartmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var department = _userStore.UpdateDepartment(id, request);
            if (department is null)
            {
                return NotFound(new { message = "Department not found." });
            }

            var updatedDepartment = _userStore.GetDepartmentById(id);
            return updatedDepartment is null
                ? NotFound(new { message = "Department not found." })
                : Ok(ToResponse(updatedDepartment));
        }
        catch (InvalidOperationException ex) when (ex.Message == "DepartmentAlreadyExists")
        {
            return Conflict(new { message = "A department with that name already exists." });
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return Conflict(new { message = "A department with that name already exists." });
        }
    }

    [HttpDelete("{id:int}")]
    public IActionResult DeleteDepartment([FromRoute] int id)
    {
        try
        {
            return _userStore.DeleteDepartment(id)
                ? NoContent()
                : NotFound(new { message = "Department not found." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "DepartmentInUse")
        {
            return Conflict(new { message = "Reassign users before deleting this department." });
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            return Conflict(new { message = "Reassign users before deleting this department." });
        }
    }

    private static DepartmentResponse ToResponse(Department department) =>
        new(department.DepartmentId, department.Name, department.Description, department.Users.Count);
}
