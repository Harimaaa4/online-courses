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

                    // 1. Если файл выбран
                    if (model.AvatarFile != null)
                    {
                        // Определяем путь к папке wwwroot
                        // Если WebRootPath пустой, берем текущую папку + wwwroot
                        string webRootPath = _appEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                        string folderPath = Path.Combine(webRootPath, "images", "avatars");

                        // Создаем папку, если её нет
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        // Уникальное имя файла
                        string uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(model.AvatarFile.FileName);

                        // Полный путь для сохранения
                        string fullPath = Path.Combine(folderPath, uniqueName);

                        // Сохраняем файл
                        using (var fileStream = new FileStream(fullPath, FileMode.Create))
                        {
                            await model.AvatarFile.CopyToAsync(fileStream);
                        }

                        // Путь для базы данных (относительный)
                        newAvatarPath = "/images/avatars/" + uniqueName;
                    }
                    // 2. Если файла нет, проверяем ссылку
                    else if (!string.IsNullOrEmpty(model.AvatarUrl))
                    {
                        newAvatarPath = model.AvatarUrl;
                    }

                    // Если данные изменились — сохраняем
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
                // Если всё же упадет — покажет ошибку на экране, а не Connection Failure
                return Content($"КРИТИЧЕСКАЯ ОШИБКА:\n{ex.Message}\n\nПУТЬ:\n{_appEnvironment?.WebRootPath ?? "NULL"}");
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