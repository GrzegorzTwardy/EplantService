using Microsoft.AspNetCore.Mvc;
using UserDomain.enums;
using UserApplication;
using UserDomain.exceptions;
using UserDomain.Models;
using Microsoft.AspNetCore.Authorization;

namespace UserService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RegisterController : ControllerBase
{
    private readonly IRegisterService _registerService;

    public RegisterController(IRegisterService registerService)
    {
        _registerService = registerService;
    }

    // function for registration based on user role
    private async Task<ActionResult> RegisterWithRole(RegisterRequest request, UserRole role)
    {
        try
        {
            var token = await _registerService.RegisterAsync(request, role);
            return Ok(new { token });
        }
        catch (InvalidCredentialsException)
        {
            return BadRequest(new { error = "A user with this username already exists" });
        }
        catch (ArgumentNullException)
        {
            return BadRequest(new { error = "Invalid username or password" });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, new { error = "Internal server error while registering user" });
        }
    }

    [HttpPost("client")]
    [AllowAnonymous]
    public Task<ActionResult> RegisterClient([FromBody] RegisterRequest request) =>
    RegisterWithRole(request, UserRole.Client);

    [HttpPost("employee")]
    [Authorize(Roles = "Admin,Employee")]
    public Task<ActionResult> RegisterEmployee([FromBody] RegisterRequest request) =>
        RegisterWithRole(request, UserRole.Employee);

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    public Task<ActionResult> RegisterAdmin([FromBody] RegisterRequest request) =>
        RegisterWithRole(request, UserRole.Admin);

    // get all users
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetUsers()
    {
        var users = await _registerService.GetAllUsers();
        return Ok(users);
    }
}
