using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using online_courses.Models;
using online_courses.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SiteInformation()
        {
            return View();
        }

        public IActionResult Courses()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            ViewData["ActivePage"] = "Contact";
            ViewData["ShowHeaderImage"] = "false";

            if (TempData["Success"] != null)
            {
                ViewData["Success"] = TempData["Success"];
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(string Name, string Email, string Subject, string Message)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Email))
            {
                // Здесь можно добавить логику отправки сообщения администратору
                TempData["Success"] = $"Спасибо, {Name}! Мы свяжемся с вами скоро.";
            }
            else
            {
                TempData["Success"] = "Пожалуйста, заполните все обязательные поля корректно.";
            }

            return RedirectToAction("Contact");
        }

        // =============================================
        // МЕТОДЫ АВТОРИЗАЦИИ И РЕГИСТРАЦИИ
        // =============================================

        // 1. РЕГИСТРАЦИЯ (Отправка письма)
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            // Проверка стандартных атрибутов ([Required], [MinLength] и т.д.)
            if (ModelState.IsValid)
            {
                // Вызов сервиса (валидация FluentValidation внутри)
                var response = await _accountService.Register(model);

                if (response.StatusCode == online_courses.Response.StatusCode.OK)
                {
                    // Успех -> формируем данные для окна подтверждения
                    var confirmModel = new ConfirmEmailViewModel
                    {
                        Email = model.Email,
                        Login = model.Login,
                        Password = model.Password, // Передаем пароль, чтобы сохранить его позже
                        PasswordConfirm = model.PasswordConfirm,
                        GeneratedCode = response.Data.GeneratedCode // Код из сервиса
                    };
                    return Ok(confirmModel);
                }

                // Ошибка от сервиса (например, "Почта занята")
                return BadRequest(new { description = response.Description });
            }

            // Сбор ошибок валидации модели в одну строку для JS
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(new { description = errors });
        }

        // 2. ПОДТВЕРЖДЕНИЕ ПОЧТЫ (Финальная регистрация)
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
                // Входим в систему (выдаем куки)
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(response.Data));

                return Ok(new { description = response.Description });
            }
            return BadRequest(new { description = response.Description });
        }

        // 3. ВХОД
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

            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(new { description = errors });
        }

        // 4. ВЫХОД
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
    }
}