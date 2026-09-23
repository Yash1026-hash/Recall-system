using Microsoft.AspNetCore.Mvc;
using RecallOperations.Api.Services;
using VehicleRecall.Shared.Models;

namespace RecallOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserStore _userStore;

    public UsersController(UserStore userStore)
    {
        _userStore = userStore;
    }

   [HttpPost("login")]
public ActionResult<UserAccount> Login([FromBody] LoginRequest request)
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

    return Ok(user);
}
[HttpPost("register")]
public ActionResult<UserAccount> Register([FromBody] RegisterRequest request)
{
    if (!ModelState.IsValid)
    {
        return ValidationProblem(ModelState);
    }

    var result = _userStore.RegisterCustomer(request);

    if (result.User is not null)
    {
        return Ok(result.User);
    }

    return result.Error switch
    {
        "EmailNotFound" =>
            NotFound(new
            {
                message = "This email was not found in the recall customer list."
            }),

        "EmailAlreadyRegistered" =>
            Conflict(new
            {
                message = "An account already exists for this email."
            }),

        "UsernameAlreadyUsed" =>
            Conflict(new
            {
                message = "That username is already in use."
            }),

        _ =>
            BadRequest(new
            {
                message = result.Error
            })
    };
}
}