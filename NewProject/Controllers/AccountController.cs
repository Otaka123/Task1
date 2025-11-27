using Application.Common;
using Application.Interfaces;
using Domain.DTOS.User.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace NewProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<Infrastructure.Identity.Admin> _userManager;
        private readonly IAppLocalizer _localizer;

        public AccountController(
            IAccountService accountService,
            ILogger<AccountController> logger,
            UserManager<Infrastructure.Identity.Admin> userManager,
           IAppLocalizer localizer)
        {
            _accountService = accountService;
            _logger = logger;
            _userManager = userManager;
            _localizer = localizer;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginRequest());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest model, string returnUrl = null)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Index", "PurchaseOrder");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _accountService.SignInAsync(model);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("User logged in successfully.");

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "PurchaseOrder");
                }

                TempData["Error"] = "فشل في تسجيل الدخول";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login.");
                TempData["Error"] = "فشل في تسجيل الدخول";
                return View(model);
            }
        }

    

        // POST: /Account/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _accountService.SignOutAsync();
                _logger.LogInformation("User logged out successfully.");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout.");
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

      
    }
}
