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
public class MaterialsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MaterialsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }

    // GET: api/materials
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MaterialDto>>> GetMaterials([FromQuery] string? article = null)
    {
        var query = _context.Materials.AsQueryable();

        // If article parameter is provided, search by article
        if (!string.IsNullOrEmpty(article))
        {
            query = query.Where(m => m.Article.Contains(article));
        }

        var materials = await query
            .OrderBy(m => m.Article)
            .ToListAsync();

        var materialDtos = materials.Select(m => new MaterialDto
        {
            Id = m.Id,
            Article = m.Article,
            Name = m.Name,
            Price = m.Price,
            Unit = m.Unit,
            CreatedAt = m.CreatedAt
        }).ToList();

        return Ok(materialDtos);
    }

    // GET: api/materials/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MaterialDto>> GetMaterial(int id)
    {
        var material = await _context.Materials.FindAsync(id);

        if (material == null)
        {
            return NotFound();
        }

        var materialDto = new MaterialDto
        {
            Id = material.Id,
            Article = material.Article,
            Name = material.Name,
            Price = material.Price,
            Unit = material.Unit,
            CreatedAt = material.CreatedAt
        };

        return Ok(materialDto);
    }

    // POST: api/materials
    [HttpPost]
    public async Task<ActionResult<MaterialDto>> CreateMaterial(CreateMaterialDto createMaterialDto)
    {
        // Check if article already exists
        var existingMaterial = await _context.Materials
            .FirstOrDefaultAsync(m => m.Article == createMaterialDto.Article);

        if (existingMaterial != null)
        {
            return BadRequest(new { message = "Материал с таким артикулом уже существует" });
        }

        var userId = GetCurrentUserId();
        var material = new Material
        {
            Article = createMaterialDto.Article,
            Name = createMaterialDto.Name,
            Price = createMaterialDto.Price,
            Unit = createMaterialDto.Unit,
            CreatedAt = DateOnly.FromDateTime(DateTime.Now),
            CreatedBy = userId
        };

        _context.Materials.Add(material);
        await _context.SaveChangesAsync();

        // Audit log
        var auditCreate = new AuditLog
        {
            EntityType = "Material",
            EntityId = material.Id,
            Action = "Create",
            UserId = userId,
            Timestamp = DateTime.Now,
            Details = System.Text.Json.JsonSerializer.Serialize(new
            {
                New = new { material.Id, material.Article, material.Name, material.Price, material.Unit }
            })
        };
        _context.AuditLogs.Add(auditCreate);
        await _context.SaveChangesAsync();

        var materialDto = new MaterialDto
        {
            Id = material.Id,
            Article = material.Article,
            Name = material.Name,
            Price = material.Price,
            Unit = material.Unit,
            CreatedAt = material.CreatedAt
        };

        return CreatedAtAction(nameof(GetMaterial), new { id = material.Id }, materialDto);
    }

    // PUT: api/materials/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMaterial(int id, UpdateMaterialDto updateMaterialDto)
    {
        var material = await _context.Materials.FindAsync(id);

        if (material == null)
        {
            return NotFound();
        }

        // Check if article already exists for another material
        var existingMaterial = await _context.Materials
            .FirstOrDefaultAsync(m => m.Article == updateMaterialDto.Article && m.Id != id);

        if (existingMaterial != null)
        {
            return BadRequest(new { message = "Материал с таким артикулом уже существует" });
        }

        var userId = GetCurrentUserId();

        var oldValues = new
        {
            material.Article,
            material.Name,
            material.Price,
            material.Unit
        };

        material.Article = updateMaterialDto.Article;
        material.Name = updateMaterialDto.Name;
        material.Price = updateMaterialDto.Price;
        material.Unit = updateMaterialDto.Unit;
        material.UpdatedBy = userId;
        material.UpdatedAt = DateTime.Now;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MaterialExists(id))
            {
                return NotFound();
            }
            throw;
        }

        var auditUpdate = new AuditLog
        {
            EntityType = "Material",
            EntityId = material.Id,
            Action = "Update",
            UserId = userId,
            Timestamp = DateTime.Now,
            Details = System.Text.Json.JsonSerializer.Serialize(new
            {
                Old = oldValues,
                New = new { material.Article, material.Name, material.Price, material.Unit }
            })
        };
        _context.AuditLogs.Add(auditUpdate);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/materials/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMaterial(int id)
    {
        var material = await _context.Materials.FindAsync(id);
        if (material == null)
        {
            return NotFound();
        }

        // Check if material is used in calculations
        var isUsedInCalculations = await _context.CalculationItems
            .AnyAsync(ci => ci.MaterialId == id);

        if (isUsedInCalculations)
        {
            return BadRequest(new { message = "Невозможно удалить материал, так как он используется в расчетах" });
        }

        _context.Materials.Remove(material);

        var userId = GetCurrentUserId();
        var auditDelete = new AuditLog
        {
            EntityType = "Material",
            EntityId = material.Id,
            Action = "Delete",
            UserId = userId,
            Timestamp = DateTime.Now,
            Details = System.Text.Json.JsonSerializer.Serialize(new
            {
                Deleted = new { material.Id, material.Article, material.Name, material.Price, material.Unit }
            })
        };
        _context.AuditLogs.Add(auditDelete);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool MaterialExists(int id)
    {
        return _context.Materials.Any(e => e.Id == id);
    }
}

