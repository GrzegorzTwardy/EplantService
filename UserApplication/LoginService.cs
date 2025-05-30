using UserDomain.exceptions;
using UserDomain.Models;
using UserDomain.repositories;

namespace UserApplication;

public interface ILoginService
{
    // returns token after correct login or throw exception
    public Task<string> LoginAsync(LoginRequest loginRequest);
}

public class LoginService : ILoginService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _userRepository;

    public LoginService(IJwtTokenService jwtTokenService, IUserRepository userRepository)
    {
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
    }

    public async Task<string> LoginAsync(LoginRequest loginRequest)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(loginRequest.Username) ||
            string.IsNullOrWhiteSpace(loginRequest.Password))
            throw new InvalidCredentialsException();

        // Find user by username
        var user = await _userRepository.GetByUsernameAsync(loginRequest.Username);

        if (user == null) throw new InvalidCredentialsException();

        // Validate password
        if (user.Password != loginRequest.Password) throw new InvalidCredentialsException();

        // Create roles list based on user role
        var roles = new List<string> { user.Role.ToString() };

        // Generate and return JWT token
        return _jwtTokenService.GenerateToken(user.Id, roles);
    }
}