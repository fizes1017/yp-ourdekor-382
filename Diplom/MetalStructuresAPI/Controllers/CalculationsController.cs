using MetalStructuresAPI.Data;
using MetalStructuresAPI.DTOs;
using MetalStructuresAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MetalStructuresAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalculationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CalculationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }

    // GET: api/calculations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CalculationDto>>> GetCalculations()
    {
        var calculations = await _context.Calculations
            .Include(c => c.CalculationItems)
                .ThenInclude(ci => ci.Material)
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

    // GET: api/calculations/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CalculationDto>> GetCalculation(int id)
    {
        var calculation = await _context.Calculations
            .Include(c => c.CalculationItems)
                .ThenInclude(ci => ci.Material)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (calculation == null)
        {
            return NotFound();
        }

        var calculationDto = new CalculationDto
        {
            Id = calculation.Id,
            TotalAmount = calculation.TotalAmount,
            CalculatedAt = calculation.CalculatedAt,
            Items = calculation.CalculationItems.Select(ci => new CalculationItemDto
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
        };

        return Ok(calculationDto);
    }

    // POST: api/calculations
    [HttpPost]
    public async Task<ActionResult<CalculationDto>> CreateCalculation(CreateCalculationDto createCalculationDto)
    {
        try
    {
        if (createCalculationDto.Items == null || !createCalculationDto.Items.Any())
        {
            return BadRequest(new { message = "Расчет должен содержать хотя бы одну позицию" });
        }

        // Validate materials exist
        var materialIds = createCalculationDto.Items.Select(i => i.MaterialId).ToList();
        var materials = await _context.Materials
            .Where(m => materialIds.Contains(m.Id))
            .ToListAsync();

        if (materials.Count != materialIds.Distinct().Count())
        {
            return BadRequest(new { message = "Один или несколько материалов не найдены" });
        }

        decimal totalAmount = 0;
        var calculationItems = new List<CalculationItem>();
            var userId = GetCurrentUserId();

            // Validate user exists and is valid
            if (userId <= 0)
            {
                return Unauthorized(new { message = "Неверный идентификатор пользователя" });
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return Unauthorized(new { message = "Пользователь не найден" });
            }

        foreach (var item in createCalculationDto.Items)
        {
            if (item.Quantity <= 0)
            {
                return BadRequest(new { message = "Количество должно быть больше нуля" });
            }

            var material = materials.First(m => m.Id == item.MaterialId);
            var unitPrice = material.Price;
            var totalPrice = unitPrice * item.Quantity;

            calculationItems.Add(new CalculationItem
            {
                MaterialId = item.MaterialId,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    CreatedBy = userId > 0 ? userId : null
            });

            totalAmount += totalPrice;
        }

        var calculation = new Calculation
        {
            TotalAmount = totalAmount,
                CalculatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                ManagerId = userId,
            CalculationItems = calculationItems
        };

        _context.Calculations.Add(calculation);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(calculation)
            .Collection(c => c.CalculationItems)
            .Query()
            .Include(ci => ci.Material)
            .LoadAsync();

        var calculationDto = new CalculationDto
        {
            Id = calculation.Id,
            TotalAmount = calculation.TotalAmount,
            CalculatedAt = calculation.CalculatedAt,
            Items = calculation.CalculationItems.Select(ci => new CalculationItemDto
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
        };

        return CreatedAtAction(nameof(GetCalculation), new { id = calculation.Id }, calculationDto);
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error creating calculation: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return StatusCode(500, new { message = "Ошибка при создании расчета: " + ex.Message });
        }
    }

    // DELETE: api/calculations/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCalculation(int id)
    {
        var calculation = await _context.Calculations.FindAsync(id);
        if (calculation == null)
        {
            return NotFound();
        }

        _context.Calculations.Remove(calculation);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}


