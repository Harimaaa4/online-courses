using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using online_courses.Models;
using online_courses.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace online_courses.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IMapper _mapper;

        // Внедряем IMapper через конструктор
        public CartController(ICartService cartService, IMapper mapper)
        {
            _cartService = cartService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. Получаем данные из сервиса (Logic/Domain)
            var response = await _cartService.GetUserItems(User.Identity.Name);

            // 2. Проверяем статус
            if (response.StatusCode == online_courses.Response.StatusCode.OK)
            {
                // 3. Маппим: AutoMapper сам преобразует список Domain.Cart в список CartViewModel,
                // используя правила, которые мы описали в AppMappingProfile.
                var viewModel = _mapper.Map<List<CartViewModel>>(response.Data);

                return View(viewModel);
            }

            // Если произошла ошибка
            return View("Error");
        }

        [HttpGet]
        public async Task<IActionResult> Add(Guid id)
        {
            var response = await _cartService.AddToCart(User.Identity.Name, id);

            if (response.StatusCode == online_courses.Response.StatusCode.OK)
            {
                return RedirectToAction("Index");
            }

            // Если не вышло добавить (например, курс уже в корзине), возвращаем пользователя к списку курсов
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
    }
}