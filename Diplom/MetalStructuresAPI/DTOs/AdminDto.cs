using MetalStructuresAPI.Models;

namespace MetalStructuresAPI.DTOs;

public class AdminUserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Manager";
    public DateOnly CreatedAt { get; set; }
}

public class UpdateManagerDto
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? FullName { get; set; }
    public string? Role { get; set; }
}

public class AdminResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class MaterialChangeLogDto
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? UserFullName { get; set; }
    public string? UserEmail { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
    public string? Summary { get; set; }
}

public class ManagerActivityDto
{
    public int ManagerId { get; set; }
    public string ManagerFullName { get; set; } = string.Empty;
    public string ManagerEmail { get; set; } = string.Empty;
    public int CalculationsCount { get; set; }
    public int CommercialProposalsCount { get; set; }
    public decimal CommercialProposalsTotalAmount { get; set; }
}

public class ManagerReportRequestDto
{
    public List<int> ManagerIds { get; set; } = new();
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

