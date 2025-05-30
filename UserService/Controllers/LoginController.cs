using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserApplication;
using UserDomain.exceptions;
using UserDomain.Models;

namespace UserService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly ILoginService _loginService;

    public LoginController(ILoginService loginService)
    {
        _loginService = loginService;
    }

    // POST api/login
    [HttpPost]
    public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        try
        {
            var token = await _loginService.LoginAsync(loginRequest);
            return Ok(new { token });
        }
        catch (InvalidCredentialsException)
        {
            return Unauthorized(new { error = "Invalid username or password" });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
            return StatusCode(500, new { error = "An error occurred during login" });
        }
    }

    // GET api/login/Admin
    [HttpGet("Admin")]
    [Authorize]
    [Authorize(Policy = "Admin")]
    public IActionResult AdminPage()
    {
        return Ok("Admin only page");
    }

    // GET api/login/Employee
    [HttpGet("Employee")]
    [Authorize]
    [Authorize(Policy = "Employee")]
    public IActionResult EmployeePage()
    {
        return Ok("Employee and Admin page");
    }

    // GET api/login/Employee
    [HttpGet("Client")]
    [Authorize]
    [Authorize(Policy = "Client")]
    public IActionResult ClientPage()
    {
        return Ok("Client and Employee and Admin page");
    }
}