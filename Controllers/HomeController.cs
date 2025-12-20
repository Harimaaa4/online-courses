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
using online_courses.Models.Filters;

namespace online_courses.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAccountService _accountService;
        private readonly ICategoryService _categoryService;
        private readonly ICourseService _courseService;
        private readonly IMapper _mapper;

        public HomeController(ILogger<HomeController> logger,
                              IAccountService accountService,
                              ICategoryService categoryService,
                              ICourseService courseService,
                              IMapper mapper)
        {
            _logger = logger;
            _accountService = accountService;
            _categoryService = categoryService;
            _courseService = courseService;
            _mapper = mapper;
        }

        public IActionResult Index() => View();
        public IActionResult SiteInformation() => View();

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

        [HttpGet]
        public async Task<IActionResult> ListCourses(Guid categoryId)
        {
            if (categoryId == Guid.Empty) return RedirectToAction("Courses");

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

        // === AUTH METHODS ===
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

            var errorList = ModelState.Keys
                .Where(k => ModelState[k].Errors.Count > 0)
                .Select(k => new {
                    field = k,
                    message = ModelState[k].Errors.First().ErrorMessage
                })
                .ToList();
            return BadRequest(errorList);
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
            var errorList = ModelState.Keys
                .Where(k => ModelState[k].Errors.Count > 0)
                .Select(k => new {
                    field = k,
                    message = ModelState[k].Errors.First().ErrorMessage
                })
                .ToList();
            return BadRequest(errorList);
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

        // === ИСПРАВЛЕННЫЙ МЕТОД GOOGLE RESPONSE ===
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded) result = await HttpContext.AuthenticateAsync("Google");

            if (result?.Principal != null)
            {
                var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
                var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;

                // 1. ИЗВЛЕКАЕМ КАРТИНКУ ИЗ GOOGLE
                var googlePicture = result.Principal.FindFirst("urn:google:picture")?.Value;

                if (email != null)
                {
                    var user = new online_courses.Domain.User
                    {
                        Login = name ?? email,
                        Email = email,
                        Password = Guid.NewGuid().ToString(),
                        Role = "User",
                        // 2. СОХРАНЯЕМ ЕЁ В ОБЪЕКТЕ ПОЛЬЗОВАТЕЛЯ (для записи в БД)
                        ImagePath = googlePicture
                    };

                    var response = await _accountService.IsCreatedAccount(user);
                    if (response.StatusCode == online_courses.Response.StatusCode.OK)
                    {
                        // 3. ДОБАВЛЯЕМ КАРТИНКУ В COOKIE ПРИ ВХОДЕ

                        // === ИСПРАВЛЕНИЕ: Убрали .Identity, так как Data это уже ClaimsIdentity ===
                        var claimsIdentity = response.Data;
                        // =========================================================================

                        if (!string.IsNullOrEmpty(googlePicture))
                        {
                            if (!claimsIdentity.HasClaim(c => c.Type == "urn:google:picture"))
                            {
                                claimsIdentity.AddClaim(new Claim("urn:google:picture", googlePicture));
                            }
                            // Также добавим как AvatarUrl для совместимости с нашей логикой
                            if (!claimsIdentity.HasClaim(c => c.Type == "AvatarUrl"))
                            {
                                claimsIdentity.AddClaim(new Claim("AvatarUrl", googlePicture));
                            }
                        }

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(response.Data));

                        return RedirectToAction("Index");
                    }
                }
            }
            return RedirectToAction("Index");
        }
        // ==========================================

        [HttpPost]
        public async Task<IActionResult> GetCoursesByFilter([FromBody] CourseFilter filter)
        {
            var response = await _courseService.GetCoursesByFilter(filter);

            if (response.StatusCode == online_courses.Response.StatusCode.OK)
            {
                var viewModels = _mapper.Map<List<CourseViewModel>>(response.Data);
                return Ok(viewModels);
            }

            return BadRequest(new { description = response.Description });
        }

        [HttpGet]
        public async Task<IActionResult> GetCourse(Guid id)
        {
            var response = await _courseService.GetCourse(id);

            if (response.StatusCode == online_courses.Response.StatusCode.OK)
            {
                var viewModel = _mapper.Map<CourseViewModel>(response.Data);
                return View(viewModel);
            }

            return RedirectToAction("Index");
        }
    }
}