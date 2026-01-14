using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MebelMaster.Models;
using System.Threading.Tasks;

namespace MebelMaster.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Input_Email, string Input_Password, bool Input_RememberMe = false)
        {
            // Простая проверка - основная валидация на клиенте
            if (string.IsNullOrEmpty(Input_Email) || string.IsNullOrEmpty(Input_Password))
            {
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(Input_Email, Input_Password, Input_RememberMe, false);
            
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            
            // Простое сообщение об ошибке
            ModelState.AddModelError(string.Empty, "Неверный email или пароль");
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string Input_Email, string Input_Password, string Input_ConfirmPassword)
        {
            // Базовая проверка - основная валидация на клиенте
            if (string.IsNullOrEmpty(Input_Email) || string.IsNullOrEmpty(Input_Password) || Input_Password != Input_ConfirmPassword)
            {
                return View();
            }

            var user = new ApplicationUser { UserName = Input_Email, Email = Input_Email };
            var result = await _userManager.CreateAsync(user, Input_Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Home");
            }

            // Простые сообщения об ошибках
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}