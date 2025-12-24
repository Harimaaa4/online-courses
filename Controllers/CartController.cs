using AutoMapper;
using Microsoft.AspNetCore.Authorization; // Обязательно для [Authorize]
using Microsoft.AspNetCore.Mvc;
using online_courses.Entities; // Для OrderDb
using online_courses.Interfaces; // Для IBaseStorage
using online_courses.Models;
using online_courses.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace online_courses.Controllers
{
    [Authorize] // Защищаем весь контроллер: только для вошедших
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IMapper _mapper;

        // Добавляем хранилища для реализации заказа
        private readonly IBaseStorage<OrderDb> _orderStorage;
        private readonly IBaseStorage<UserDb> _userStorage;
        private readonly IBaseStorage<CartDb> _cartStorage;

        public CartController(ICartService cartService,
                              IMapper mapper,
                              IBaseStorage<OrderDb> orderStorage,
                              IBaseStorage<UserDb> userStorage,
                              IBaseStorage<CartDb> cartStorage)
        {
            _cartService = cartService;
            _mapper = mapper;
            _orderStorage = orderStorage;
            _userStorage = userStorage;
            _cartStorage = cartStorage;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Пытаемся получить Email. Если его нет, берем Name.
            var userName = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity.Name;

            var response = await _cartService.GetUserItems(userName);

            if (response.StatusCode == online_courses.Response.StatusCode.OK)
            {
                var viewModel = _mapper.Map<List<CartViewModel>>(response.Data);
                return View(viewModel);
            }

            ViewBag.ErrorDescription = response.Description;
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> Add(Guid id)
        {
            var userName = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity.Name;

            var response = await _cartService.AddToCart(userName, id);

            if (response.StatusCode == online_courses.Response.StatusCode.OK)
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Courses", "Home");
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _cartService.DeleteFromCart(id);

            if (response.StatusCode == online_courses.Response.StatusCode.OK)
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        // === НОВЫЙ МЕТОД: ОФОРМЛЕНИЕ ЗАКАЗА ===
        public async Task<IActionResult> CreateOrder()
        {
            // 1. Получаем текущего пользователя
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity.Name;
            var users = await _userStorage.GetAllAsync();
            var user = users.FirstOrDefault(x => x.Email == userEmail || x.Login == userEmail);

            if (user == null) return RedirectToAction("Index", "Home");

            // 2. Получаем товары из корзины (прямой запрос к базе для точности)
            var allCartItems = await _cartStorage.GetAllAsync();
            var myCart = allCartItems.Where(x => x.UserId == user.Id).ToList();

            if (!myCart.Any()) return RedirectToAction("Index"); // Корзина пуста

            // 3. Собираем данные для заказа
            // (В CartDb у нас есть навигационное свойство Course, если оно подгружено. 
            // Если Course null, значит нужно проверить Include в Storage, но для простоты берем пока так)

            // Считаем сумму. Если Course null (не подгрузился), считаем 0 (или можно сделать доп. запрос)
            decimal totalPrice = 0;
            List<string> courseNamesList = new List<string>();

            // Получаем курсы отдельно, чтобы точно знать цены, если CartDb не подтянул Course
            // (Это более надежный способ, если в CartStorage нет .Include(x => x.Course))
            // Но пока предположим, что данные есть или возьмем их из сервиса

            // Упрощенный вариант через Service (так как там точно есть данные)
            var serviceResponse = await _cartService.GetUserItems(userEmail);
            if (serviceResponse.StatusCode == online_courses.Response.StatusCode.OK)
            {
                var cartItems = serviceResponse.Data;
                totalPrice = cartItems.Sum(x => x.Course.Price);
                courseNamesList = cartItems.Select(x => x.Course.Name).ToList();
            }

            string courseNames = string.Join(", ", courseNamesList);

            // 4. Создаем заказ
            var newOrder = new OrderDb
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                UserEmail = user.Email,
                CourseNames = courseNames,
                TotalPrice = totalPrice,
                CreatedDate = DateTime.UtcNow,
                Status = "Оплачен" // Сразу ставим оплачен
            };

            await _orderStorage.AddAsync(newOrder);

            // 5. Очищаем корзину
            foreach (var item in myCart)
            {
                await _cartStorage.DeleteAsync(item);
            }

            // 6. Перенаправляем на страницу благодарности
            return RedirectToAction("ThankYou");
        }

        // Страница "Спасибо за покупку"
        public IActionResult ThankYou()
        {
            return View();
        }
    }
}