using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ourdekor.Data;
using ourdekor.Models;

namespace ourdekor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductMaterialsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductMaterialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductMaterials>>> GetProductMaterials()
        {
            return await _context.ProductMaterials
                .Include(pm => pm.Products)
                .Include(pm => pm.Materials)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductMaterials>> GetProductMaterial(int id)
        {
            var productMaterial = await _context.ProductMaterials
                .Include(pm => pm.Products)
                    .ThenInclude(p => p.ProductType)
                .Include(pm => pm.Materials)
                    .ThenInclude(m => m.MaterialType)
                .FirstOrDefaultAsync(pm => pm.id == id);

            if (productMaterial == null)
            {
                return NotFound();
            }
            return productMaterial;
        }

        [HttpPost]
        public async Task<ActionResult<ProductMaterials>> PostProductMaterial(ProductMaterials productMaterial)
        {
            // количество материала должно быть больше 0
            if (productMaterial.count <= 0)
            {
                return BadRequest("Количество материала должно быть положительным числом");
            }

            _context.ProductMaterials.Add(productMaterial);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductMaterial), new { id = productMaterial.id }, productMaterial);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductMaterial(int id, ProductMaterials productMaterial)
        {
            if (id != productMaterial.id)
            {
                return BadRequest();
            }

            _context.Entry(productMaterial).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProductMaterial(int id)
        {
            var productMaterial = await _context.ProductMaterials.FindAsync(id);
            if (productMaterial == null)
            {
                return NotFound();
            }

            _context.ProductMaterials.Remove(productMaterial);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}