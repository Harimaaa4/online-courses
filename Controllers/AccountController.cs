using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting; // Для работы с путями
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using online_courses.Entities;
using online_courses.Interfaces;
using online_courses.Models;
using System;
using System.Collections.Generic;
using System.IO; // Для работы с файлами
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace online_courses.Controllers
{
    public class AccountController : Controller
    {
        private readonly IBaseStorage<UserDb> _userStorage;
        private readonly IWebHostEnvironment _appEnvironment;

        // Конструктор
        public AccountController(IBaseStorage<UserDb> userStorage, IWebHostEnvironment appEnvironment)
        {
            _userStorage = userStorage;
            _appEnvironment = appEnvironment;
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid) return Ok(model);
            return BadRequest(ModelState);
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

        // === БЕЗОПАСНЫЙ МЕТОД ЗАГРУЗКИ ===
        // === ИСПРАВЛЕННЫЙ МЕТОД ЗАГРУЗКИ ===
        // === ИСПРАВЛЕННЫЙ МЕТОД (принимаем файл напрямую) ===
        [HttpPost]
        public async Task<IActionResult> Profile(IFormFile avatarFile, string avatarUrl)
        {
            try
            {
                var userName = User.Identity.Name;
                var users = await _userStorage.GetAllAsync();
                var user = users.FirstOrDefault(x => x.Login == userName || x.Email == userName);

                if (user != null)
                {
                    string newAvatarPath = null;

                    // 1. Если файл пришел (avatarFile не null)
                    if (avatarFile != null)
                    {
                        // Определяем путь. Если WebRootPath не задан, берем текущую папку + wwwroot
                        string webRootPath = _appEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                        // Путь: wwwroot/images/avatars
                        string uploadDir = Path.Combine(webRootPath, "images", "avatars");

                        // На всякий случай проверяем папку
                        if (!Directory.Exists(uploadDir))
                        {
                            Directory.CreateDirectory(uploadDir);
                        }

                        // Генерируем имя файла
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                        string filePath = Path.Combine(uploadDir, fileName);

                        // Сохраняем
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await avatarFile.CopyToAsync(stream);
                        }

                        // Записываем путь для базы
                        newAvatarPath = "/images/avatars/" + fileName;
                    }
                    // 2. Если файла нет, берем ссылку
                    else if (!string.IsNullOrEmpty(avatarUrl))
                    {
                        newAvatarPath = avatarUrl;
                    }

                    // Сохраняем в базу, если что-то поменялось
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
                return Content($"ОШИБКА: {ex.Message}");
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
            if (!string.IsNullOrEmpty(user.ImagePath)) claims.Add(new Claim("AvatarUrl", user.ImagePath));

            var oldGooglePic = User.FindFirst("urn:google:picture");
            if (oldGooglePic != null) claims.Add(new Claim("urn:google:picture", oldGooglePic.Value));

            var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}