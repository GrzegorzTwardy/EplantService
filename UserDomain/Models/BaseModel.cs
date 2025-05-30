using System.ComponentModel.DataAnnotations;

namespace UserDomain.Models;

public class BaseModel
{
    [Key] public int Id { get; set; }

    public bool Deleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid CreatedBy { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid UpdatedBy { get; set; }
}