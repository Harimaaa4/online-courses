using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using online_courses.Models;
using online_courses.Services.Interfaces;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using AutoMapper;

namespace online_courses.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAccountService _accountService;
        private readonly ICategoryService _categoryService;
        private readonly ICourseService _courseService; // <--- НОВОЕ
        private readonly IMapper _mapper;

        // Обновленный конструктор с ICourseService
        public HomeController(ILogger<HomeController> logger,
                              IAccountService accountService,
                              ICategoryService categoryService,
                              ICourseService courseService, // <--- НОВОЕ
                              IMapper mapper)
        {
            _logger = logger;
            _accountService = accountService;
            _categoryService = categoryService;
            _courseService = courseService; // <--- НОВОЕ
            _mapper = mapper;
        }

        public IActionResult Index() => View();
        public IActionResult SiteInformation() => View();

        // 1. КАТАЛОГ (КАТЕГОРИИ)
        [HttpGet]
        public async Task<IActionResult> Courses()
        {
            var response = await _categoryService.GetAllCategories();
            if (response.StatusCode == online_courses.Response.StatusCode.OK)
            {
                var viewModels = _mapper.Map<List<CategoryViewModel>>(response.Data);
                return View(viewModels);
            }
            return View(new List<CategoryViewModel>());
        }

        // 2. СПИСОК КУРСОВ ПО КАТЕГОРИИ (ГЛАВА 21)
        [HttpGet]
        public async Task<IActionResult> ListCourses(Guid categoryId)
        {
            if (categoryId == Guid.Empty) return RedirectToAction("Courses");

            // Получаем курсы конкретной категории
            var response = await _courseService.GetCoursesByCategory(categoryId);

            if (response.StatusCode == online_courses.Response.StatusCode.OK)
            {
                var viewModels = _mapper.Map<List<CourseViewModel>>(response.Data);
                return View(viewModels);
            }

            return RedirectToAction("Courses");
        }

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

        // === AUTH METHODS (ОСТАЮТСЯ ПРЕЖНИМИ) ===
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
                        Password = Guid.NewGuid().ToString(),
                        Role = "User"
                    };
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