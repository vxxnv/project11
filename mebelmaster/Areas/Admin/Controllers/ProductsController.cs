using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MebelMaster.Data;
using MebelMaster.Models;
using MebelMaster.Services;

namespace MebelMaster.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ApplicationDbContext context, IFileService fileService, ILogger<ProductsController> logger)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .ToListAsync();
            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, List<IFormFile> images)
        {
            Console.WriteLine("=== ПОЛУЧЕНЫ ДАННЫЕ ФОРМЫ ===");
            Console.WriteLine($"Name: {product.Name}");
            Console.WriteLine($"Article: {product.Article}");
            Console.WriteLine($"Description: {product.Description}");
            Console.WriteLine($"Price: {product.Price}");
            Console.WriteLine($"CategoryId: {product.CategoryId}");
            Console.WriteLine($"IsPopular: {product.IsPopular}");
            Console.WriteLine($"IsActive: {product.IsActive}");

            // Ручная проверка CategoryId
            if (product.CategoryId == 0)
            {
                ModelState.AddModelError("CategoryId", "Выберите категорию");
                Console.WriteLine("CategoryId равен 0 - категория не выбрана");
            }
            else
            {
                // Проверяем, есть ли категория с таким ID
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == product.CategoryId);
                Console.WriteLine($"Категория существует в БД: {categoryExists}");

                if (!categoryExists)
                {
                    ModelState.AddModelError("CategoryId", "Выбранная категория не существует");
                }
            }

            // Убираем проверку ModelState.IsValid и делаем свою
            var isValid = ModelState.ErrorCount == 0;
            Console.WriteLine($"Модель валидна: {isValid}");

            if (isValid)
            {
                try
                {
                    Console.WriteLine("Модель валидна, сохраняем товар...");
                    
                    product.CreatedAt = DateTime.UtcNow;
                    product.UpdatedAt = DateTime.UtcNow;

                    _context.Add(product);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"Товар успешно создан с ID: {product.Id}");

                    if (images != null && images.Any())
                    {
                        foreach (var image in images)
                        {
                            if (image.Length > 0)
                            {
                                var imageUrl = await _fileService.SaveImageAsync(image, "images/products");
                                var productImage = new ProductImage
                                {
                                    ProductId = product.Id,
                                    ImageUrl = imageUrl,
                                    AltText = product.Name,
                                    DisplayOrder = 0
                                };
                                _context.ProductImages.Add(productImage);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Товар успешно создан";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при сохранении: {ex.Message}");
                    ModelState.AddModelError("", "Произошла ошибка при создании товара");
                }
            }
            else
            {
                Console.WriteLine("Модель не валидна. Ошибки:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($" - {error.ErrorMessage}");
                }
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            Console.WriteLine($"Загружено категорий для ViewBag: {ViewBag.Categories.Count}");
            
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, List<IFormFile> images)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Сохраняем оригинальные даты
                    var existingProduct = await _context.Products.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == id);
                        
                    if (existingProduct != null)
                    {
                        product.CreatedAt = existingProduct.CreatedAt;
                    }
                    
                    product.UpdatedAt = DateTime.UtcNow;

                    _context.Update(product);

                    // Добавляем новые изображения
                    if (images != null && images.Any())
                    {
                        foreach (var image in images)
                        {
                            if (image.Length > 0)
                            {
                                var imageUrl = await _fileService.SaveImageAsync(image, "images/products");
                                var productImage = new ProductImage
                                {
                                    ProductId = product.Id,
                                    ImageUrl = imageUrl,
                                    AltText = product.Name,
                                    DisplayOrder = 0
                                };
                                _context.ProductImages.Add(productImage);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Товар успешно обновлен";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            // Подсчитываем связанные заказы
            var ordersCount = await _context.Orders
                .CountAsync(o => o.ProductId == id);
            ViewBag.RelatedOrdersCount = ordersCount;

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (product != null)
            {
                try
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    
                    // 1. Отвязываем заказы от товара (устанавливаем ProductId в null)
                    var relatedOrders = await _context.Orders
                        .Where(o => o.ProductId == id)
                        .ToListAsync();

                    foreach (var order in relatedOrders)
                    {
                        order.ProductId = null;
                        order.Product = null;
                    }

                    // 2. Удаляем изображения товара
                    foreach (var image in product.Images)
                    {
                        _fileService.DeleteFile(image.ImageUrl);
                    }
                    _context.ProductImages.RemoveRange(product.Images);

                    // 3. Удаляем товар
                    _context.Products.Remove(product);
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    TempData["SuccessMessage"] = "Товар успешно удален. Связанные заказы сохранены.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при удалении товара {ProductId}", id);
                    TempData["ErrorMessage"] = $"Не удалось удалить товар: {ex.Message}";
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image != null)
            {
                _fileService.DeleteFile(image.ImageUrl);
                _context.ProductImages.Remove(image);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}