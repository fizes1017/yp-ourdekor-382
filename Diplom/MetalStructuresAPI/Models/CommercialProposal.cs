using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalStructuresAPI.Models;

[Table("commercial_proposals")]
public class CommercialProposal
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("calculationid")]
    public int CalculationId { get; set; }

    [Required]
    [Column("managerid")]
    public int ManagerId { get; set; }

    [Required]
    [StringLength(255)]
    [Column("customercompany")]
    public string CustomerCompany { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    [Column("customerperson")]
    public string CustomerPerson { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Column("customerphone")]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Column("customeremail")]
    public string CustomerEmail { get; set; } = string.Empty;

    [StringLength(255)]
    [Column("customeraddress")]
    public string? CustomerAddress { get; set; }

    [StringLength(50)]
    [Column("proposalnumber")]
    public string? ProposalNumber { get; set; }

    [Column("createdat", TypeName = "timestamp")]
    public DateTime CreatedAt { get; set; }

    [Column("comments", TypeName = "text")]
    public string? Comments { get; set; }

    [ForeignKey("CalculationId")]
    public virtual Calculation Calculation { get; set; } = null!;

    [ForeignKey("ManagerId")]
    public virtual User Manager { get; set; } = null!;
}
