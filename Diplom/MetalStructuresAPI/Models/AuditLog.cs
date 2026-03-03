using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalStructuresAPI.Models;

[Table("audit_log")]
public class AuditLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Column("entitytype")]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    [Column("entityid")]
    public int EntityId { get; set; }

    [Required]
    [StringLength(20)]
    [Column("action")]
    public string Action { get; set; } = string.Empty;

    [Column("userid")]
    public int? UserId { get; set; }

    [Column("timestamp", TypeName = "timestamp")]
    public DateTime Timestamp { get; set; }

    [Column("details", TypeName = "text")]
    public string? Details { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
