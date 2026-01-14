using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MebelMaster.Data;
using MebelMaster.Models;

namespace MebelMaster.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                PopularProducts = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Category)
                    .Where(p => p.IsPopular && p.IsActive)
                    .Take(8)
                    .ToListAsync(),
                Reviews = await _context.Reviews
                    .Where(r => r.IsApproved)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(6)
                    .ToListAsync()
            };

            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult Contacts()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitOrder(Order order)
        {
            if (ModelState.IsValid)
            {
                order.CreatedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                order.Status = OrderStatus.New;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ваша заявка отправлена. Мы свяжемся с вами в ближайшее время.";
                return RedirectToAction("Index");
            }

            return View("Contacts", order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(Review review)
        {
            if (ModelState.IsValid)
            {
                review.CreatedAt = DateTime.UtcNow;
                review.IsApproved = false;

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Спасибо за ваш отзыв! Он будет опубликован после проверки модератором.";
                return RedirectToAction("Index");
            }

            return View("Index", new HomeViewModel
            {
                PopularProducts = await _context.Products
                    .Include(p => p.Images)
                    .Where(p => p.IsPopular && p.IsActive)
                    .Take(8)
                    .ToListAsync(),
                Reviews = await _context.Reviews
                    .Where(r => r.IsApproved)
                    .Take(6)
                    .ToListAsync()
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}