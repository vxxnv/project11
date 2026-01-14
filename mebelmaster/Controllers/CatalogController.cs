using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MebelMaster.Data;
using MebelMaster.Models;

namespace MebelMaster.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(ApplicationDbContext context, ILogger<CatalogController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? categoryId, string sortOrder = "name", string searchString = "", int page = 1)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = sortOrder == "name" ? "name_desc" : "name";
            ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";
            ViewData["CurrentFilter"] = searchString;

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.IsActive);

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString));
            }

            products = sortOrder switch
            {
                "name_desc" => products.OrderByDescending(p => p.Name),
                "price" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                _ => products.OrderBy(p => p.Name),
            };

            int pageSize = 9;
            var count = await products.CountAsync();
            var items = await products.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var model = new CatalogViewModel
            {
                Products = items,
                Categories = await _context.Categories.ToListAsync(),
                SelectedCategoryId = categoryId,
                PageViewModel = new PageViewModel(count, page, pageSize),
                SortOrder = sortOrder,
                SearchString = searchString
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            // Похожие товары
            var relatedProducts = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsActive)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            if (ModelState.IsValid)
            {
                order.CreatedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                order.Status = OrderStatus.New;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ваша заявка отправлена. Мы свяжемся с вами в ближайшее время.";
                return RedirectToAction("Details", new { id = order.ProductId });
            }

            return RedirectToAction("Details", new { id = order.ProductId });
        }
    }
}