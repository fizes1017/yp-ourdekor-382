using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ourdekor.Data;
using ourdekor.Models;

namespace ourdekor.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/products-list")]
        [HttpGet("/")]
        public async Task<IActionResult> IndexView(string search = "", int? typeId = null, string sort = "")
        {
            var query = _context.Products
                .Include(p => p.ProductType)
                .Include(p => p.ProductMaterials!).ThenInclude(pm => pm.Materials)
                .AsQueryable();

            // поиск по названии
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.name.ToLower().Contains(search.ToLower()));
            
            // фильтрация
            if (typeId.HasValue)
                query = query.Where(p => p.ProductTypeId == typeId);

            // сортировка
            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.min_price),
                "price_desc" => query.OrderByDescending(p => p.min_price),
                "name_asc" => query.OrderBy(p => p.name),
                _ => query.OrderBy(p => p.id)
            };

            var products = await query.ToListAsync();
            return View("Index", products);
        }

        [HttpGet("/product/edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.ProductTypes = await _context.ProductTypes.ToListAsync();

            if (!id.HasValue || id == 0)
            {
                return View("AddEdit", new Products());
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.id == id);

            if (product == null) return NotFound();

            return View("AddEdit", product);
        }

        [HttpPost("/product/save")]
        public async Task<IActionResult> Save(Products product)
        {
            ModelState.Remove("ProductType");
            ModelState.Remove("ProductMaterials");

            if (product.min_price < 0)
                ModelState.AddModelError("min_price", "Цена не может быть отрицательной");

            if (ModelState.IsValid)
            {
                if (product.id == 0)
                {
                    _context.Products.Add(product);
                }
                else
                {
                    _context.Products.Update(product);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("IndexView");
            }

            ViewBag.ProductTypes = await _context.ProductTypes.ToListAsync();
            return View("AddEdit", product);
        }

        [HttpGet("/product/materials/{id}")]
        public async Task<IActionResult> ShowMaterials(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductMaterials!).ThenInclude(pm => pm.Materials)
                .FirstOrDefaultAsync(p => p.id == id);

            if (product == null) return NotFound();
            return View("MaterialsList", product);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProductsApi(string search = "", int? typeId = null, string sort = "")
        {
            var query = _context.Products
                .Include(p => p.ProductType)
                .Include(p => p.ProductMaterials!).ThenInclude(pm => pm.Materials)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.name.ToLower().Contains(search.ToLower()));

            if (typeId.HasValue)
                query = query.Where(p => p.ProductTypeId == typeId);

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.min_price),
                "price_desc" => query.OrderByDescending(p => p.min_price),
                "name_asc" => query.OrderBy(p => p.name),
                _ => query.OrderBy(p => p.id)
            };

            var products = await query.ToListAsync();

            var result = products.Select(p => new {
                p.id,
                p.name,
                p.article,
                p.min_price,
                typeName = p.ProductType?.name,
                // ?. и ?? для безопасного обращения к материалам
                productionCost = p.ProductMaterials?.Sum(pm => (double)pm.count * (double)(pm.Materials?.price ?? 0)) ?? 0
            });

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

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
                return Ok(-1);

            // количество должно быть увеличено с учетом брака
            double countWithScrap = (param1 * param2 * (double)pType.coefficient) * (1 + (double)mType.defect_percent / 100);
            
            // возвращаем целое число, округленное вверх
            return Ok((int)Math.Ceiling(countWithScrap * quantity));
        }
    }
}