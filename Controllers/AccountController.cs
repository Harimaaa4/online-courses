using System.Linq;
using Microsoft.AspNetCore.Mvc;
using online_courses.Models; // Подключаем наши модели

namespace online_courses.Controllers
{
    public class AccountController : Controller
    {
        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Позже (в Главе 14) здесь будет настоящая логика входа
                return Ok(model); // Возвращаем 200 OK
            }

            // Создание списка ошибок
            var errors = ModelState.Keys
                .Where(k => ModelState[k].Errors.Count > 0)
                .Select(k => new {
                    Field = k, // e.g., "Email" or "Password"
                    Message = ModelState[k].Errors.First().ErrorMessage
                })
                .ToList();
            return BadRequest(errors); // Возвращаем 400 Bad Request с ошибками
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register([FromBody] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Позже (в Главе 14) здесь будет настоящая логика регистрации
                return Ok(model); // Возвращаем 200 OK
            }

            // Создание списка ошибок
            var errors = ModelState.Keys
                .Where(k => ModelState[k].Errors.Count > 0)
                .Select(k => new {
                    Field = k, // e.g., "Login" or "PasswordConfirm"
                    Message = ModelState[k].Errors.First().ErrorMessage
                })
                .ToList();
            return BadRequest(errors); // Возвращаем 400 Bad Request с ошибками
        }
    }
}