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
        public async Task<ActionResult<IEnumerable<object>>> GetProducts(string search = "", int? typeId = null, string sort = "")
        {
            // запрос с загрузкой всех связей
            var query = _context.Products
                .Include(p => p.ProductType)
                .Include(p => p.ProductMaterials)
                    .ThenInclude(pm => pm.Materials)
                .AsQueryable();

            // поиск по названи.
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.name.ToLower().Contains(search.ToLower()));
            }

            // фильтрация
            if (typeId.HasValue)
            {
                query = query.Where(p => p.ProductTypeId == typeId);
            }

            // сортировка
            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.min_price),
                "price_desc" => query.OrderByDescending(p => p.min_price),
                "name_asc" => query.OrderBy(p => p.name),
                _ => query.OrderBy(p => p.id) // по умолчанию
            };

            var products = await query.ToListAsync();

            // ормируем результат с расчетом стоимости
            var result = products.Select(p => new {
                p.id,
                p.name,
                p.article,
                p.min_price,
                p.width,
                typeName = p.ProductType?.name,
                // расчет стоимости производства (сумма материалов)
                productionCost = p.ProductMaterials.Sum(pm =>
                    (double)pm.count * (double)(pm.Materials?.price ?? 0))
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Products>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductType) 
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

        [HttpGet("calculate-material")]
        public async Task<ActionResult<int>> GetCalculation(int productTypeId, int materialTypeId, int quantity, double param1, double param2)
        {
            var pType = await _context.ProductTypes.FindAsync(productTypeId);
            var mType = await _context.MaterialTypes.FindAsync(materialTypeId);

            if (pType == null || mType == null || quantity <= 0 || param1 <= 0 || param2 <= 0)
            {
                return Ok(-1);
            }

            double countPerOne = param1 * param2 * (double)pType.coefficient;

            // количество должно быть увеличено с учетом брака
            double countWithScrap = countPerOne * (1 + (double)mType.defect_percent / 100);

            double total = countWithScrap * quantity;

            // возвращаем целое число, округленное вверх
            return Ok((int)Math.Ceiling(total));
        }
    }
}
