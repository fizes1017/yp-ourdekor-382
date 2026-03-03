using MetalStructuresAPI.Data;
using MetalStructuresAPI.DTOs;
using MetalStructuresAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MetalStructuresAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordService _passwordService;

    public ProfileController(ApplicationDbContext context, PasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    // GET: api/profile
    [HttpGet]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "Профиль не найден" });
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            FullName = user.FullName,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        return Ok(userDto);
    }

    // PUT: api/profile
    [HttpPut]
    public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileDto updateProfileDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "Профиль не найден" });
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(updateProfileDto.FullName))
        {
            user.FullName = updateProfileDto.FullName;
        }

        if (!string.IsNullOrWhiteSpace(updateProfileDto.Phone))
        {
            // Check if phone is already taken by another user
            if (await _context.Users.AnyAsync(u => u.Phone == updateProfileDto.Phone && u.Id != userId))
            {
                return BadRequest(new { message = "Телефон уже используется другим пользователем" });
            }
            user.Phone = updateProfileDto.Phone;
        }

        await _context.SaveChangesAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            FullName = user.FullName,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        return Ok(userDto);
    }

    // POST: api/profile/change-password
    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "Профиль не найден" });
        }

        // Verify current password
        if (!_passwordService.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(new { message = "Текущий пароль неверен" });
        }

        // Validate password confirmation
        if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
        {
            return BadRequest(new { message = "Пароли не совпадают" });
        }

        // Validate new password
        if (string.IsNullOrWhiteSpace(changePasswordDto.NewPassword) || changePasswordDto.NewPassword.Length < 6)
        {
            return BadRequest(new { message = "Новый пароль должен содержать минимум 6 символов" });
        }

        // Update password
        user.PasswordHash = _passwordService.HashPassword(changePasswordDto.NewPassword);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Пароль успешно изменен" });
    }

    // GET: api/profile/calculations
    [HttpGet("calculations")]
    public async Task<ActionResult<IEnumerable<CalculationDto>>> GetMyCalculations()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        var calculations = await _context.Calculations
            .Include(c => c.CalculationItems)
                .ThenInclude(ci => ci.Material)
            .Where(c => c.ManagerId == userId)
            .OrderByDescending(c => c.CalculatedAt)
            .ToListAsync();

        var calculationDtos = calculations.Select(c => new CalculationDto
        {
            Id = c.Id,
            TotalAmount = c.TotalAmount,
            CalculatedAt = c.CalculatedAt,
            Items = c.CalculationItems.Select(ci => new CalculationItemDto
            {
                Id = ci.Id,
                MaterialId = ci.MaterialId,
                MaterialName = ci.Material.Name,
                MaterialArticle = ci.Material.Article,
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice,
                TotalPrice = ci.TotalPrice,
                Unit = ci.Material.Unit
            }).ToList()
        }).ToList();

        return Ok(calculationDtos);
    }
}

