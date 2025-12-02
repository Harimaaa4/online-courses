using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google; // Один раз
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using online_courses.Models;
using online_courses.Services.Interfaces;
using System; // Для Guid
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Diagnostics;

namespace online_courses.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAccountService _accountService;

        public HomeController(ILogger<HomeController> logger, IAccountService accountService)
        {
            _logger = logger;
            _accountService = accountService;
        }

        public IActionResult Index() => View();
        public IActionResult SiteInformation() => View();
        public IActionResult Courses() => View();
        public IActionResult Privacy() => View();

        [HttpGet]
        public IActionResult Contact()
        {
            ViewData["ActivePage"] = "Contact";
            ViewData["ShowHeaderImage"] = "false";
            if (TempData["Success"] != null) ViewData["Success"] = TempData["Success"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(string Name, string Email, string Subject, string Message)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Email))
                TempData["Success"] = $"Спасибо, {Name}! Мы свяжемся с вами скоро.";
            else
                TempData["Success"] = "Пожалуйста, заполните все обязательные поля корректно.";
            return RedirectToAction("Contact");
        }

        // =============================================
        // AUTH METHODS
        // =============================================

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await _accountService.Register(model);
                if (response.StatusCode == online_courses.Response.StatusCode.OK)
                {
                    var confirmModel = new ConfirmEmailViewModel
                    {
                        Email = model.Email,
                        Login = model.Login,
                        Password = model.Password,
                        PasswordConfirm = model.PasswordConfirm,
                        GeneratedCode = response.Data.GeneratedCode
                    };
                    return Ok(confirmModel);
                }
                return BadRequest(new { description = response.Description });
            }
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(new { description = errors });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailViewModel model)
        {
            var user = new online_courses.Domain.User
            {
                Email = model.Email,
                Login = model.Login,
                Password = model.Password,
                GeneratedCode = model.GeneratedCode,
                Role = "User"
            };

            var response = await _accountService.ConfirmEmail(user, model.CodeConfirm);

            if (response.StatusCode == online_courses.Response.StatusCode.OK)
            {
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(response.Data));
                return Ok(new { description = response.Description });
            }
            return BadRequest(new { description = response.Description });
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await _accountService.Login(model);
                if (response.StatusCode == online_courses.Response.StatusCode.OK)
                {
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(response.Data));
                    return Ok(new { description = "Успешный вход" });
                }
                return BadRequest(new { description = response.Description });
            }
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(new { description = errors });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // =============================================
        // GOOGLE AUTH
        // =============================================

        public async Task AuthenticationGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") });
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded) result = await HttpContext.AuthenticateAsync("Google");

            if (result?.Principal != null)
            {
                var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
                var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

                if (email != null)
                {
                    var user = new online_courses.Domain.User
                    {
                        Login = name ?? email,
                        Email = email,
                        Password = Guid.NewGuid().ToString(), // Случайный пароль
                        Role = "User"
                    };

                    // Здесь вызываем метод IsCreatedAccount.
                    // Он принимает Domain.User, как и положено.
                    var response = await _accountService.IsCreatedAccount(user);

                    if (response.StatusCode == online_courses.Response.StatusCode.OK)
                    {
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(response.Data));
                        return RedirectToAction("Index");
                    }
                }
            }
            return RedirectToAction("Index");
        }
    }
}