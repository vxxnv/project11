using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MebelMaster.Data;
using MebelMaster.Models;

namespace MebelMaster.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ApplicationDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.Products)
                .ToListAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.ParentCategories = _context.Categories.Where(c => c.ParentCategoryId == null).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;

                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ParentCategories = _context.Categories.Where(c => c.ParentCategoryId == null).ToList();
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            
            ViewBag.ParentCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == null && c.Id != id) // Исключаем текущую категорию
                .ToListAsync();
                
            // Получаем количество товаров в категории
            ViewBag.ProductsCount = await _context.Products
                .CountAsync(p => p.CategoryId == id);
                
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Сохраняем оригинальные даты создания
                    var existingCategory = await _context.Categories.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Id == id);
                        
                    if (existingCategory != null)
                    {
                        category.CreatedAt = existingCategory.CreatedAt;
                    }
                    
                    category.UpdatedAt = DateTime.UtcNow;

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Категория успешно обновлена";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            ViewBag.ParentCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == null && c.Id != id)
                .ToListAsync();
                
            ViewBag.ProductsCount = await _context.Products
                .CountAsync(p => p.CategoryId == id);
                
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ActionName("Delete")] // Это отличает метод от GET версии
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (category != null)
            {
                try
                {
                    // Удаляем все подкатегории
                    if (category.Children != null && category.Children.Any())
                    {
                        _context.Categories.RemoveRange(category.Children);
                    }

                    // Устанавливаем CategoryId в 0 для товаров
                    if (category.Products != null && category.Products.Any())
                    {
                        foreach (var product in category.Products)
                        {
                            product.CategoryId = 0;
                        }
                    }

                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Категория успешно удалена";
                }
                catch (DbUpdateException ex)
                {
                    TempData["ErrorMessage"] = "Не удалось удалить категорию. Возможно, она содержит товары, которые нельзя удалить.";
                    _logger.LogError(ex, "Ошибка при удалении категории {CategoryId}", id);
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }
            
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> CategoryExists(int id)
        {
            return await _context.Categories.AnyAsync(e => e.Id == id);
        }
    }
}