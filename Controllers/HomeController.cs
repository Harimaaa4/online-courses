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

        // === МЕТОДЫ АВТОРИЗАЦИИ И РЕГИСТРАЦИИ ===

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Вызываем сервис. Он отправит письмо, но НЕ сохранит в базу пока что.
                var response = await _accountService.Register(model);

                if (response.StatusCode == online_courses.Response.StatusCode.OK)
                {
                    // 2. Если письмо ушло успешно, готовим данные для подтверждения
                    // Мы "запоминаем" сгенерированный код и данные пользователя, чтобы передать их на фронтенд
                    // (Примечание: в реальных проектах код на фронт лучше не передавать в открытую, но делаем по методике)

                    // Ручной маппинг или через AutoMapper (если настроили)
                    var confirmModel = new ConfirmEmailViewModel
                    {
                        Email = model.Email,
                        Login = model.Login,
                        Password = model.Password,
                        PasswordConfirm = model.PasswordConfirm,
                        GeneratedCode = response.Data.GeneratedCode // Берем код из ответа сервиса
                    };

                    return Ok(confirmModel); // Отправляем это обратно в JavaScript
                }
                return BadRequest(new { description = response.Description });
            }

            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(new { description = errors });
        }

        // НОВЫЙ МЕТОД: Подтверждение почты
        [HttpPost]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailViewModel model)
        {
            // Превращаем ViewModel обратно в доменную модель User
            var user = new online_courses.Domain.User
            {
                Email = model.Email,
                Login = model.Login,
                // Пароль уже захеширован в сервисе Register? 
                // В нашей реализации Register в AccountService мы хешировали newUser.Password.
                // Но response.Data вернул объект с уже захешированным паролем?
                // В AccountService.Register мы делали: newUser.Password = HashHelper(pass); return ... Data = newUser;
                // Значит в model.Password сейчас лежит уже ХЕШ?
                // Нет, с фронта придет то, что мы отправили. 
                // ВАЖНО: AccountService.ConfirmEmail снова вызывает Authenticate(user).
                // Давайте доверимся сервису.
                Password = model.Password,
                GeneratedCode = model.GeneratedCode,
                Role = "User"
            };

            // Вызываем финальное подтверждение в сервисе
            var response = await _accountService.ConfirmEmail(user, model.CodeConfirm);

            if (response.StatusCode == online_courses.Response.StatusCode.OK)
            {
                // Если всё ок - выдаем куки (входим на сайт)
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

                    return Ok(new { description = response.Description });
                }
                return BadRequest(new { description = response.Description });
            }

            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(new { description = errors });
        }
        // Метод выхода из системы
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }

}