using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecallOperations.Api.Services;
using VehicleRecall.Shared.Models;

namespace RecallOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserStore _userStore;
    private readonly JwtTokenService _jwtTokenService;

    public UsersController(UserStore userStore, JwtTokenService jwtTokenService)
    {
        _userStore = userStore;
        _jwtTokenService = jwtTokenService;
    }

    [HttpGet]
    [Authorize(Policy = "ManagerOnly")]
    public ActionResult<IReadOnlyList<UserSummaryResponse>> GetUsers(
        [FromQuery] string? role,
        [FromQuery] string? registrationStatus,
        [FromQuery] string? search,
        [FromQuery] string? vin,
        [FromQuery] string? vehicleStatus,
        [FromQuery] string? campaign,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? departmentId = null,
        [FromQuery] bool? isActive = null)
    {
        var users = _userStore.SearchUsers(
                role,
                registrationStatus,
                search,
                vin,
                vehicleStatus,
                campaign,
                sort,
                page,
                pageSize,
                departmentId,
                isActive)
            .Select(UserSummaryResponse.From)
            .ToList();

        return Ok(users);
    }

    [HttpGet("roles")]
    [Authorize(Policy = "ManagerOnly")]
    public ActionResult<IReadOnlyList<string>> GetRoles() =>
        Ok(_userStore.GetRoles().Select(role => role.Name).ToList());

    [HttpGet("{id:int}")]
    [Authorize(Policy = "ManagerOnly")]
    public ActionResult<UserSummaryResponse> GetUserById([FromRoute] int id)
    {
        var user = _userStore.GetById(id);

        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(UserSummaryResponse.From(user));
    }

    [HttpPost("staff")]
    [Authorize(Policy = "ManagerOnly")]
    public ActionResult<UserSummaryResponse> CreateStaffUser([FromBody] StaffUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var user = _userStore.CreateStaffUser(request);
            return CreatedAtAction(nameof(GetUserById),
                new { id = user.UserId },
                UserSummaryResponse.From(user));
        }
        catch (InvalidOperationException ex) when (ex.Message == "UsernameAlreadyUsed")
        {
            return Conflict(new { message = "That username is already in use." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "EmailAlreadyRegistered")
        {
            return Conflict(new { message = "An account already exists for this email." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "DepartmentNotFound")
        {
            return BadRequest(new { message = "Choose an existing department." });
        }
    }

    [HttpPatch("{id:int}/role")]
    [Authorize(Policy = "ManagerOnly")]
    public ActionResult<UserSummaryResponse> UpdateUserRole(
        [FromRoute] int id,
        [FromBody] UpdateUserRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = _userStore.UpdateUserRole(id, request.Role);
        return user is null
            ? NotFound(new { message = "User not found." })
            : Ok(UserSummaryResponse.From(user));
    }

    [HttpPut("{id:int}/roles")]
    [Authorize(Policy = "ManagerOnly")]
    public ActionResult<UserSummaryResponse> UpdateUserRoles(
        [FromRoute] int id,
        [FromBody] UpdateUserRolesRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var user = _userStore.UpdateUserRoles(id, request.Roles);
            return user is null
                ? NotFound(new { message = "User not found." })
                : Ok(UserSummaryResponse.From(user));
        }
        catch (InvalidOperationException ex) when (ex.Message == "RoleNotFound")
        {
            return BadRequest(new { message = "Choose only roles that exist." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "AtLeastOneRoleRequired")
        {
            return BadRequest(new { message = "A user must have at least one role." });
        }
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Policy = "ManagerOnly")]
    public ActionResult<UserSummaryResponse> UpdateUserStatus(
        [FromRoute] int id,
        [FromBody] UpdateUserStatusRequest request)
    {
        var user = _userStore.UpdateUserStatus(id, request.IsActive);
        return user is null
            ? NotFound(new { message = "User not found." })
            : Ok(UserSummaryResponse.From(user));
    }

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and password are required." });
        }

        var user = _userStore.ValidateCredentials(request.Username, request.Password);

        if (user is null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        return Ok(_jwtTokenService.Create(user));
    }

    [HttpPost("register")]
    public ActionResult<UserSummaryResponse> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = _userStore.RegisterCustomer(request);

        if (result.User is not null)
        {
            return CreatedAtAction(nameof(GetUserById),
                new { id = result.User.UserId },
                UserSummaryResponse.From(result.User));
        }

        return result.Error switch
        {
            "EmailNotFound" => NotFound(new
            {
                message = "This email was not found in the recall customer list."
            }),

            "EmailAlreadyRegistered" => Conflict(new
            {
                message = "An account already exists for this email."
            }),

            "UsernameAlreadyUsed" => Conflict(new
            {
                message = "That username is already in use."
            }),

            _ => BadRequest(new { message = result.Error })
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "ManagerOnly")]
    public ActionResult<UserSummaryResponse> UpdateUser([FromRoute] int id, [FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var updatedUser = _userStore.UpdateUser(id, request);

            if (updatedUser is null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(UserSummaryResponse.From(updatedUser));
        }
        catch (InvalidOperationException ex) when (ex.Message == "UsernameAlreadyUsed")
        {
            return Conflict(new { message = "That username is already in use." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "EmailAlreadyRegistered")
        {
            return Conflict(new { message = "An account already exists for this email." });
        }
    }

    [HttpPatch("{id:int}/profile")]
    [Authorize(Policy = "ManagerOnly")]
    public ActionResult<UserSummaryResponse> UpdateUserProfile(
        [FromRoute] int id,
        [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var user = _userStore.UpdateUserProfile(id, request);
            return user is null
                ? NotFound(new { message = "User not found." })
                : Ok(UserSummaryResponse.From(user));
        }
        catch (InvalidOperationException ex) when (ex.Message == "UsernameAlreadyUsed")
        {
            return Conflict(new { message = "That username is already in use." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "EmailAlreadyRegistered")
        {
            return Conflict(new { message = "An account already exists for this email." });
        }
        catch (InvalidOperationException ex) when (ex.Message == "DepartmentNotFound")
        {
            return BadRequest(new { message = "Choose an existing department." });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ManagerOnly")]
    public IActionResult DeleteUser([FromRoute] int id)
    {
        var deleted = _userStore.DeleteUser(id);

        if (!deleted)
        {
            return NotFound(new { message = "User not found." });
        }

        return NoContent();
    }
}