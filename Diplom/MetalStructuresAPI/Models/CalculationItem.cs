using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalStructuresAPI.Models;

[Table("calculation_items")]
public class CalculationItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("calculationid")]
    public int CalculationId { get; set; }

    [Required]
    [Column("materialid")]
    public int MaterialId { get; set; }

    [Required]
    [Column("quantity", TypeName = "decimal(10,3)")]
    public decimal Quantity { get; set; }

    [Required]
    [Column("unitprice", TypeName = "decimal(10,2)")]
    public decimal UnitPrice { get; set; }

    [Required]
    [Column("totalprice", TypeName = "decimal(10,2)")]
    public decimal TotalPrice { get; set; }

    [Column("createdat", TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    [Column("createdby")]
    public int? CreatedBy { get; set; }

    [Column("updatedat", TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    [Column("updatedby")]
    public int? UpdatedBy { get; set; }

    // Navigation properties
    [ForeignKey("CalculationId")]
    public virtual Calculation Calculation { get; set; } = null!;

    [ForeignKey("MaterialId")]
    public virtual Material Material { get; set; } = null!;

    [ForeignKey("CreatedBy")]
    public virtual User? Creator { get; set; }

    [ForeignKey("UpdatedBy")]
    public virtual User? Updater { get; set; }
}


