using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MebelMaster.Data;

namespace MebelMaster.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("/test/db")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                // Проверяем подключение к БД
                var canConnect = await _context.Database.CanConnectAsync();

                // Пробуем получить категории
                var categories = await _context.Categories.ToListAsync();

                return Json(new
                {
                    Success = true,
                    CanConnect = canConnect,
                    CategoriesCount = categories.Count,
                    Categories = categories
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }
    }
}