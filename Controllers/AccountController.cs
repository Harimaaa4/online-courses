using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting; // <--- НУЖНО ДЛЯ РАБОТЫ С ФАЙЛАМИ
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using online_courses.Entities;
using online_courses.Interfaces;
using online_courses.Models;
using System;
using System.Collections.Generic;
using System.IO; // <--- НУЖНО ДЛЯ ПОТОКОВ
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace online_courses.Controllers
{
    public class AccountController : Controller
    {
        private readonly IBaseStorage<UserDb> _userStorage;
        private readonly IWebHostEnvironment _appEnvironment; // <--- СЕРВИС ОКРУЖЕНИЯ

        // Добавляем IWebHostEnvironment в конструктор
        public AccountController(IBaseStorage<UserDb> userStorage)
        {
            _userStorage = userStorage;
            _appEnvironment = null;
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid) return Ok(model);
            return BadRequest(ModelState); // Упростил для краткости
        }

        [HttpPost]
        public IActionResult Register([FromBody] RegisterViewModel model)
        {
            if (ModelState.IsValid) return Ok(model);
            return BadRequest(ModelState);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userName = User.Identity.Name;
            var users = await _userStorage.GetAllAsync();
            var user = users.FirstOrDefault(x => x.Login == userName || x.Email == userName);

            if (user == null) return RedirectToAction("Index", "Home");

            var model = new ProfileViewModel
            {
                Login = user.Login,
                Email = user.Email,
                AvatarUrl = user.ImagePath ?? User.FindFirst("urn:google:picture")?.Value
            };

            return View(model);
        }

        // === ОБНОВЛЕННЫЙ МЕТОД СОХРАНЕНИЯ ПРОФИЛЯ ===
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            try
            {
                var userName = User.Identity.Name;
                var users = await _userStorage.GetAllAsync();
                var user = users.FirstOrDefault(x => x.Login == userName || x.Email == userName);

                if (user != null)
                {
                    string newAvatarPath = null;

                    // 1. Если загружен файл
                    if (model.AvatarFile != null)
                    {
                        // Проверка: есть ли папка wwwroot
                        if (string.IsNullOrEmpty(_appEnvironment.WebRootPath))
                        {
                            return Content("ОШИБКА: WebRootPath is null. Убедитесь, что папка wwwroot существует в проекте.");
                        }

                        // Генерируем уникальное имя файла
                        string uniqueName = Guid.NewGuid().ToString() + "_" + model.AvatarFile.FileName;

                        // Путь относительно сайта
                        string relativePath = "/images/avatars/" + uniqueName;

                        // Полный путь на диске (Используем Path.Combine для надежности)
                        string fullPath = Path.Combine(_appEnvironment.WebRootPath, "images", "avatars", uniqueName);

                        // Создаем папку, если её нет
                        string dirInfo = Path.GetDirectoryName(fullPath);
                        if (!Directory.Exists(dirInfo))
                        {
                            Directory.CreateDirectory(dirInfo);
                        }

                        // Сохраняем файл
                        using (var fileStream = new FileStream(fullPath, FileMode.Create))
                        {
                            await model.AvatarFile.CopyToAsync(fileStream);
                        }

                        newAvatarPath = relativePath;
                    }
                    // 2. Если файла нет, берем ссылку
                    else if (!string.IsNullOrEmpty(model.AvatarUrl))
                    {
                        newAvatarPath = model.AvatarUrl;
                    }

                    // Обновляем данные
                    if (newAvatarPath != null)
                    {
                        user.ImagePath = newAvatarPath;
                        await _userStorage.UpdateAsync(user);
                        await RefreshSignIn(user);
                    }
                }

                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                // === ВРЕМЕННЫЙ ВЫВОД ОШИБКИ НА ЭКРАН ===
                return Content($"ПРОИЗОШЛА ОШИБКА:\n{ex.Message}\n\nСТЕК:\n{ex.StackTrace}");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task RefreshSignIn(UserDb user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            if (!string.IsNullOrEmpty(user.Email)) claims.Add(new Claim(ClaimTypes.Email, user.Email));

            // Здесь теперь может лежать как URL (http...), так и путь к файлу (/images/...)
            if (!string.IsNullOrEmpty(user.ImagePath)) claims.Add(new Claim("AvatarUrl", user.ImagePath));

            var oldGooglePic = User.FindFirst("urn:google:picture");
            if (oldGooglePic != null) claims.Add(new Claim("urn:google:picture", oldGooglePic.Value));

            var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}