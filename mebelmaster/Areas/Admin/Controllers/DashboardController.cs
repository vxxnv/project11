using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MebelMaster.Data;
using MebelMaster.Models;

namespace MebelMaster.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                NewOrders = await _context.Orders.Where(o => o.Status == OrderStatus.New).CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                RecentOrders = await _context.Orders
                    .Include(o => o.Product)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(10)
                    .ToListAsync()
            };

            ViewBag.TotalProducts = model.TotalProducts;
            ViewBag.TotalOrders = model.TotalOrders;
            ViewBag.NewOrders = model.NewOrders;
            ViewBag.TotalCategories = model.TotalCategories;
            ViewBag.RecentOrders = model.RecentOrders;

            return View(model);
        }
    }

    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int NewOrders { get; set; }
        public int TotalCategories { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
    }
}