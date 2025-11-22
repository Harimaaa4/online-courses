using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using online_courses.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;



namespace online_courses.Controllers
{
    public class HomeController : Controller
    {
        // --- ВОТ ИСПРАВЛЕНИЕ ---
        [HttpGet] // <-- БЫЛО [HttpPost], СТАЛО [HttpGet]
        // GET: /Contact — отображение формы
        public IActionResult Contact()
        {
            ViewData["ActivePage"] = "Contact";
            ViewData["ShowHeaderImage"] = "false";

            // Если есть сообщение (об успехе ИЛИ ошибке) — показываем
            if (TempData["Success"] != null)
            {
                ViewData["Success"] = TempData["Success"];
            }

            return View();
        }

        // POST: /Contact — обработка формы
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(string Name, string Email, string Subject, string Message)
        {
            // Эта проверка (ModelState.IsValid) здесь почти бесполезна,
            // так как ты не используешь ViewModel.
            // Но мы можем оставить ее.
            if (ModelState.IsValid && !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Email))
            {
                // Здесь можно добавить отправку email или сохранение в БД
                TempData["Success"] = $"Спасибо, {Name}! Мы свяжемся с вами скоро.";
            }
            else
            {
                // Отправляем сообщение об ошибке
                TempData["Success"] = "Пожалуйста, заполните все обязательные поля корректно.";
            }

            return RedirectToAction("Contact");
        }

        // ... (остальной код: Courses, Index, Privacy и т.д.)

        public IActionResult Courses()
        {
            return View();
        }
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index() //главная
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        // Страница «О нас» — по методичке
        public IActionResult SiteInformation()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
