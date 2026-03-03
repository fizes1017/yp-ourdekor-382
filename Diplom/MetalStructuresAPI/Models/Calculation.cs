using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalStructuresAPI.Models;

[Table("calculations")]
public class Calculation
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("totalamount", TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    [Column("calculatedat", TypeName = "date")]
    public DateOnly CalculatedAt { get; set; }

    [Column("managerid")]
    public int? ManagerId { get; set; }

    [Column("updatedat", TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }

    [Column("updatedby")]
    public int? UpdatedBy { get; set; }

    // Navigation properties
    [ForeignKey("ManagerId")]
    public virtual User? Manager { get; set; }

    [ForeignKey("UpdatedBy")]
    public virtual User? Updater { get; set; }

    public virtual ICollection<CalculationItem> CalculationItems { get; set; } = new List<CalculationItem>();
}


