using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalStructuresAPI.Models;

[Table("company_info")]
public class CompanyInfo
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("address")]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Column("phone")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Column("inn")]
    public string Inn { get; set; } = string.Empty;

    [StringLength(20)]
    [Column("kpp")]
    public string? Kpp { get; set; }

    [Required]
    [Column("bankdetails", TypeName = "text")]
    public string BankDetails { get; set; } = string.Empty;

    [Column("updatedat", TypeName = "timestamp")]
    public DateTime? UpdatedAt { get; set; }
}
