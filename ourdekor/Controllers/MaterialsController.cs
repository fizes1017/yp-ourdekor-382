using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ourdekor.Data;
using ourdekor.Models;

namespace ourdekor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MaterialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Materials>>> GetMaterials()
        {
            return await _context.Materials.Include(p => p.MaterialType).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Materials>> GetMaterial(int id)
        {
            var material = await _context.Materials
                .Include(m => m.MaterialType)
                .FirstOrDefaultAsync(m => m.id == id);

            if (material == null)
            {
                return NotFound();
            }
            return material;
        }

        [HttpPost]
        public async Task<ActionResult<Materials>> PostMaterial(Materials material)
        {
            _context.Materials.Add(material);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMaterial),
                new { id = material.id }, material);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Materials>> PutMaterial(int id, Materials material)
        {
            if (id != material.id)
            {
                return BadRequest();
            }

            _context.Materials.Update(material);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMaterial(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }

            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
