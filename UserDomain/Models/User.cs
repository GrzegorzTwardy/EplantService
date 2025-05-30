using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using UserDomain.enums;

namespace UserDomain.Models;

[Index(nameof(Username), IsUnique = true)]
public class User : BaseModel
{
    [Required] [MinLength(1)] public UserRole Role { get; set; } = UserRole.Client;

    [Required] [MaxLength(20)] public string Username { get; set; } = default!;

    [Required] [MaxLength(40)] public string Password { get; set; } = default!;
}