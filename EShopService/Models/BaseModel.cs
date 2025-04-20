using System.ComponentModel.DataAnnotations;

namespace EShopService.Models;

public class BaseModel
{
    [Required] public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool Deleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid CreatedBy { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid UpdatedBy { get; set; }
}