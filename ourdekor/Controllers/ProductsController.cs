using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ourdekor.Data;
using ourdekor.Models;

namespace ourdekor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Products>>> GetProducts()
        {
            return await _context.Products.Include(p => p.ProductType).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Products>> GetProduct(int id)
        {
            // Вместо FindAsync используем FirstOrDefaultAsync с Include
            var product = await _context.Products
                .Include(p => p.ProductType) // Вот эта магия подтянет данные типа
                .FirstOrDefaultAsync(p => p.id == id);

            if (product == null)
            {
                return NotFound();
            }
            return product;
        }

        [HttpPost]
        public async Task<ActionResult<Products>> PostProduct(Products product)
        {
            if (product.min_price < 0)
            {
                return BadRequest("Цена не может быть отрицательной");
            }
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProduct),
                new { id = product.id }, product);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Products>> PutProduct (int id, Products product)
        {
            if (id != product.id)
            {
                return BadRequest();
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
