using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserDomain.enums;
using UserDomain.exceptions;
using UserDomain.Models;
using UserDomain.repositories;

namespace UserApplication;

public interface IRegisterService
{
    public Task<string> RegisterAsync(RegisterRequest registerRequest, UserRole role);
    public Task<IEnumerable<User>> GetAllUsers();
}

public class RegisterService : IRegisterService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _userRepository;

    public RegisterService(IJwtTokenService jwtTokenService, IUserRepository userRepository)
    {
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
    }

    public async Task<string> RegisterAsync(RegisterRequest registerRequest, UserRole role)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(registerRequest.Username) ||
            string.IsNullOrWhiteSpace(registerRequest.Password))
            throw new ArgumentNullException();

        // Check if user exists
        var existingUser = await _userRepository.GetByUsernameAsync(registerRequest.Username);
        if (existingUser != null)
            throw new InvalidCredentialsException();

        User newUser = new()
        {
            Username = registerRequest.Username,
            Password = registerRequest.Password,
            Role = role
        };

        await _userRepository.AddAsync(newUser);

        // Create roles list based on user role
        var roles = new List<string> { newUser.Role.ToString() };

        // Generate and return JWT token
        return _jwtTokenService.GenerateToken(newUser.Id, roles);
    }

    public async Task<IEnumerable<User>> GetAllUsers()
    {
        return await _userRepository.GetAllAsync();
    }
}
