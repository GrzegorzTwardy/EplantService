using System.ComponentModel.DataAnnotations;

namespace EShopDomain.Models;

public class BaseModel
{
    [Key] public int Id { get; set; }

    [Required] [MaxLength(255)] public string Name { get; set; } = string.Empty;

    public bool Deleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid CreatedBy { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid UpdatedBy { get; set; }
}