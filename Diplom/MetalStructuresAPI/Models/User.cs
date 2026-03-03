using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalStructuresAPI.Models;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    [Column("fullname")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Column("phone")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("passwordhash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Column("role")]
    public string Role { get; set; } = "Manager";

    [Column("createdat", TypeName = "date")]
    public DateOnly CreatedAt { get; set; }
}