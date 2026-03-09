using System.Security.Claims;
using System.Text.Json;
using MetalStructuresAPI.Data;
using MetalStructuresAPI.DTOs;
using MetalStructuresAPI.Models;
using MetalStructuresAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MetalStructuresAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly ManagerReportPdfService _reportService;

    public AdminController(
        ApplicationDbContext context,
        PasswordService passwordService,
        ManagerReportPdfService reportService)
    {
        _context = context;
        _passwordService = passwordService;
        _reportService = reportService;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }

    // GET: api/admin/managers
    [HttpGet("managers")]
    public async Task<ActionResult<IEnumerable<AdminUserDto>>> GetManagers()
    {
        var managers = await _context.Users
            .Where(u => u.Role == "Manager")
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var result = managers.Select(u => new AdminUserDto
        {
            Id = u.Id,
            Email = u.Email,
            Phone = u.Phone,
            FullName = u.FullName,
            Role = u.Role,
            CreatedAt = u.CreatedAt
        }).ToList();

        return Ok(result);
    }

    // PUT: api/admin/managers/5
    [HttpPut("managers/{id:int}")]
    public async Task<ActionResult<AdminUserDto>> UpdateManager(int id, UpdateManagerDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.Role != "Manager")
        {
            return NotFound(new { message = "Менеджер не найден" });
        }

        if (!string.IsNullOrWhiteSpace(dto.Email) && !string.Equals(dto.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id))
            {
                return BadRequest(new { message = "Email уже используется другим пользователем" });
            }
            user.Email = dto.Email;
        }

        if (!string.IsNullOrWhiteSpace(dto.Phone) && dto.Phone != user.Phone)
        {
            if (await _context.Users.AnyAsync(u => u.Phone == dto.Phone && u.Id != id))
            {
                return BadRequest(new { message = "Телефон уже используется другим пользователем" });
            }
            user.Phone = dto.Phone;
        }

        if (!string.IsNullOrWhiteSpace(dto.FullName))
        {
            user.FullName = dto.FullName;
        }

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            // Разрешаем только Manager и Admin
            if (dto.Role != "Manager" && dto.Role != "Admin")
            {
                return BadRequest(new { message = "Недопустимая роль" });
            }
            user.Role = dto.Role;
        }

        await _context.SaveChangesAsync();

        var result = new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            FullName = user.FullName,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        return Ok(result);
    }

    // POST: api/admin/managers/5/reset-password
    [HttpPost("managers/{id:int}/reset-password")]
    public async Task<ActionResult> ResetManagerPassword(int id, AdminResetPasswordDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.Role != "Manager")
        {
            return NotFound(new { message = "Менеджер не найден" });
        }

        if (dto.NewPassword != dto.ConfirmPassword)
        {
            return BadRequest(new { message = "Пароли не совпадают" });
        }

        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
        {
            return BadRequest(new { message = "Пароль должен содержать минимум 6 символов" });
        }

        user.PasswordHash = _passwordService.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Пароль менеджера успешно изменён" });
    }

    // GET: api/admin/material-changes
    [HttpGet("material-changes")]
    public async Task<ActionResult<IEnumerable<MaterialChangeLogDto>>> GetMaterialChanges(
        [FromQuery] int? managerId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var query = _context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.EntityType == "Material");

        if (managerId.HasValue)
        {
            query = query.Where(a => a.UserId == managerId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(a => a.Timestamp >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(a => a.Timestamp <= to.Value);
        }

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Take(500)
            .ToListAsync();

        var result = logs.Select(a => new MaterialChangeLogDto
        {
            Id = a.Id,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            Action = a.Action,
            UserId = a.UserId,
            UserFullName = a.User?.FullName,
            UserEmail = a.User?.Email,
            Timestamp = a.Timestamp,
            Details = a.Details,
            Summary = BuildMaterialChangeSummary(a.Details)
        }).ToList();

        return Ok(result);
    }

    private static string? BuildMaterialChangeSummary(string? detailsJson)
    {
        if (string.IsNullOrWhiteSpace(detailsJson))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(detailsJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("New", out var newEl) &&
                !root.TryGetProperty("Old", out _) &&
                !root.TryGetProperty("Deleted", out _))
            {
                var article = newEl.TryGetProperty("Article", out var a) ? a.GetString() : null;
                var name = newEl.TryGetProperty("Name", out var n) ? n.GetString() : null;
                var price = newEl.TryGetProperty("Price", out var p) ? p.GetDecimal() : (decimal?)null;
                var unit = newEl.TryGetProperty("Unit", out var u) ? u.GetString() : null;

                return $"Добавлен материал {name} (артикул {article}, цена {price}, ед. изм. {unit}) в справочник";
            }

            if (root.TryGetProperty("Old", out var oldEl) &&
                root.TryGetProperty("New", out var newEl2))
            {
                var changes = new List<string>();

                string? oldArticle = oldEl.TryGetProperty("Article", out var oa) ? oa.GetString() : null;
                string? newArticle = newEl2.TryGetProperty("Article", out var na) ? na.GetString() : null;
                if (oldArticle != newArticle)
                {
                    changes.Add($"артикул: {oldArticle} → {newArticle}");
                }

                string? oldName = oldEl.TryGetProperty("Name", out var on) ? on.GetString() : null;
                string? newName = newEl2.TryGetProperty("Name", out var nn) ? nn.GetString() : null;
                if (oldName != newName)
                {
                    changes.Add($"название: \"{oldName}\" → \"{newName}\"");
                }

                decimal? oldPrice = oldEl.TryGetProperty("Price", out var op) ? op.GetDecimal() : (decimal?)null;
                decimal? newPrice = newEl2.TryGetProperty("Price", out var np) ? np.GetDecimal() : (decimal?)null;
                if (oldPrice != newPrice)
                {
                    changes.Add($"цена: {oldPrice} → {newPrice}");
                }

                string? oldUnit = oldEl.TryGetProperty("Unit", out var ou) ? ou.GetString() : null;
                string? newUnit = newEl2.TryGetProperty("Unit", out var nu) ? nu.GetString() : null;
                if (oldUnit != newUnit)
                {
                    changes.Add($"ед. изм.: {oldUnit} → {newUnit}");
                }

                return changes.Count > 0
                    ? $"Изменены поля: {string.Join(", ", changes)}"
                    : "Изменений в основных полях не зафиксировано";
            }

            if (root.TryGetProperty("Deleted", out var deletedEl))
            {
                var article = deletedEl.TryGetProperty("Article", out var a) ? a.GetString() : null;
                var name = deletedEl.TryGetProperty("Name", out var n) ? n.GetString() : null;
                var price = deletedEl.TryGetProperty("Price", out var p) ? p.GetDecimal() : (decimal?)null;
                var unit = deletedEl.TryGetProperty("Unit", out var u) ? u.GetString() : null;

                return $"Удалён материал: артикул {article}, название \"{name}\", цена {price}, ед. изм. {unit}";
            }

            return detailsJson;
        }
        catch
        {
            return detailsJson;
        }
    }

    // GET: api/admin/manager-activity
    [HttpGet("manager-activity")]
    public async Task<ActionResult<IEnumerable<ManagerActivityDto>>> GetManagerActivity(
        [FromQuery] List<int> managerIds,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        if (managerIds == null || managerIds.Count == 0)
        {
            return BadRequest(new { message = "Не выбраны менеджеры" });
        }

        if (to < from)
        {
            return BadRequest(new { message = "Неверный диапазон дат" });
        }

        var fromDateOnly = DateOnly.FromDateTime(from.Date);
        var toDateOnly = DateOnly.FromDateTime(to.Date);

        var managers = await _context.Users
            .Where(u => managerIds.Contains(u.Id))
            .ToListAsync();

        var calculations = await _context.Calculations
            .Where(c => c.ManagerId != null
                        && managerIds.Contains(c.ManagerId.Value)
                        && c.CalculatedAt >= fromDateOnly
                        && c.CalculatedAt <= toDateOnly)
            .ToListAsync();

        var proposals = await _context.CommercialProposals
            .Include(cp => cp.Calculation)
            .Where(cp => managerIds.Contains(cp.ManagerId)
                         && cp.CreatedAt >= from
                         && cp.CreatedAt <= to)
            .ToListAsync();

        var result = new List<ManagerActivityDto>();

        foreach (var manager in managers)
        {
            var calcCount = calculations.Count(c => c.ManagerId == manager.Id);
            var managerProposals = proposals.Where(cp => cp.ManagerId == manager.Id).ToList();
            var proposalCount = managerProposals.Count;
            var totalAmount = managerProposals.Sum(cp => cp.Calculation.TotalAmount);

            result.Add(new ManagerActivityDto
            {
                ManagerId = manager.Id,
                ManagerFullName = manager.FullName,
                ManagerEmail = manager.Email,
                CalculationsCount = calcCount,
                CommercialProposalsCount = proposalCount,
                CommercialProposalsTotalAmount = totalAmount
            });
        }

        return Ok(result);
    }

    // POST: api/admin/manager-report
    [HttpPost("manager-report")]
    public async Task<IActionResult> CreateManagerReport([FromBody] ManagerReportRequestDto request)
    {
        if (request.ManagerIds == null || request.ManagerIds.Count == 0)
        {
            return BadRequest(new { message = "Не выбраны менеджеры" });
        }

        if (request.To < request.From)
        {
            return BadRequest(new { message = "Неверный диапазон дат" });
        }

        var fromDateOnly = DateOnly.FromDateTime(request.From.Date);
        var toDateOnly = DateOnly.FromDateTime(request.To.Date);

        var adminId = GetCurrentUserId();
        var admin = await _context.Users.FindAsync(adminId);

        var managers = await _context.Users
            .Where(u => request.ManagerIds.Contains(u.Id))
            .ToListAsync();

        var calculations = await _context.Calculations
            .Where(c => c.ManagerId != null
                        && request.ManagerIds.Contains(c.ManagerId.Value)
                        && c.CalculatedAt >= fromDateOnly
                        && c.CalculatedAt <= toDateOnly)
            .ToListAsync();

        var proposals = await _context.CommercialProposals
            .Include(cp => cp.Calculation)
            .Where(cp => request.ManagerIds.Contains(cp.ManagerId)
                         && cp.CreatedAt >= request.From
                         && cp.CreatedAt <= request.To)
            .ToListAsync();

        var stats = new List<ManagerActivityDto>();

        foreach (var manager in managers)
        {
            var calcCount = calculations.Count(c => c.ManagerId == manager.Id);
            var managerProposals = proposals.Where(cp => cp.ManagerId == manager.Id).ToList();
            var proposalCount = managerProposals.Count;
            var totalAmount = managerProposals.Sum(cp => cp.Calculation.TotalAmount);

            stats.Add(new ManagerActivityDto
            {
                ManagerId = manager.Id,
                ManagerFullName = manager.FullName,
                ManagerEmail = manager.Email,
                CalculationsCount = calcCount,
                CommercialProposalsCount = proposalCount,
                CommercialProposalsTotalAmount = totalAmount
            });
        }

        var adminName = admin?.FullName ?? admin?.Email ?? "Администратор";
        var generatedAt = DateTime.UtcNow;

        var pdf = _reportService.GenerateReport(
            adminName,
            request.From,
            request.To,
            stats);

        var fileName = $"Отчет_по_менеджерам_{generatedAt:yyyyMMdd_HHmm}.pdf";
        return File(pdf, "application/pdf", fileName);
    }
}

