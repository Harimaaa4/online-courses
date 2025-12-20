using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using online_courses.Models;
using online_courses.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics; // Нужно для Activity
using System.Threading.Tasks;
using System.Security.Claims;

namespace online_courses.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IMapper _mapper;

        public CartController(ICartService cartService, IMapper mapper)
        {
            _cartService = cartService;
            _mapper = mapper;
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
            // То же самое здесь
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
    }
}