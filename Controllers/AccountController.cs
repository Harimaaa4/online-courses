using Microsoft.AspNetCore.Mvc;
using online_courses.Entities;
using online_courses.Interfaces;
using online_courses.Models; // Подключаем наши модели
using System.Linq;
using System.Threading.Tasks;

namespace online_courses.Controllers
{
    public class AccountController : Controller
    {
        // === ДОБАВЛЯЕМ ПОЛЕ ХРАНИЛИЩА ===
        private readonly IBaseStorage<UserDb> _userStorage;

        // === СОЗДАЕМ КОНСТРУКТОР ДЛЯ ВНЕДРЕНИЯ ЗАВИСИМОСТИ ===
        public AccountController(IBaseStorage<UserDb> userStorage)
        {
            _userStorage = userStorage;
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Позже здесь будет логика входа
                return Ok(model);
            }

            var errors = ModelState.Keys
                .Where(k => ModelState[k].Errors.Count > 0)
                .Select(k => new {
                    Field = k,
                    Message = ModelState[k].Errors.First().ErrorMessage
                })
                .ToList();
            return BadRequest(errors);
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register([FromBody] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Позже здесь будет логика регистрации
                return Ok(model);
            }

            var errors = ModelState.Keys
                .Where(k => ModelState[k].Errors.Count > 0)
                .Select(k => new {
                    Field = k,
                    Message = ModelState[k].Errors.First().ErrorMessage
                })
                .ToList();
            return BadRequest(errors);
        }

        // Страница профиля (Чтение)
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userName = User.Identity.Name;

            // Теперь _userStorage доступен и не вызовет ошибку
            var users = await _userStorage.GetAllAsync();
            var user = users.FirstOrDefault(x => x.Login == userName || x.Email == userName);

            if (user == null) return RedirectToAction("Index", "Home");

            var model = new ProfileViewModel
            {
                Login = user.Login,
                Email = user.Email,
                // Если пользователь загрузил свою картинку, берем её. 
                // Если нет — пробуем взять из Google (через Claims), иначе пусто.
                AvatarUrl = user.ImagePath ?? User.FindFirst("urn:google:picture")?.Value
            };

            return View(model);
        }

        // Страница профиля (Сохранение)
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var userName = User.Identity.Name;

            var users = await _userStorage.GetAllAsync();
            var user = users.FirstOrDefault(x => x.Login == userName || x.Email == userName);

            if (user != null)
            {
                // Обновляем ссылку на картинку в базе
                user.ImagePath = model.AvatarUrl;
                await _userStorage.UpdateAsync(user);
            }

            return RedirectToAction("Profile");
        }
    }
}