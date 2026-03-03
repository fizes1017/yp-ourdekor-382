using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalStructuresAPI.Models;

[Table("materials")]
public class Material
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Column("article")]
    public string Article { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("price", TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(20)]
    [Column("unit")]
    public string Unit { get; set; } = string.Empty;

    [Column("createdat", TypeName = "date")]
    public DateOnly CreatedAt { get; set; }

    [Column("createdby")]
    public int? CreatedBy { get; set; }

    [Column("updatedat", TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    [Column("updatedby")]
    public int? UpdatedBy { get; set; }

    // Navigation properties
    [ForeignKey("CreatedBy")]
    public virtual User? Creator { get; set; }

    [ForeignKey("UpdatedBy")]
    public virtual User? Updater { get; set; }
}


