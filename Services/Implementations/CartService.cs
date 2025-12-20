using AutoMapper;
using Microsoft.EntityFrameworkCore;
using online_courses.Domain;
using online_courses.Entities;
using online_courses.Interfaces;
using online_courses.Response;
using online_courses.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace online_courses.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly IBaseStorage<CartDb> _cartStorage;
        private readonly IBaseStorage<UserDb> _userStorage;
        private readonly IBaseStorage<CourseDb> _courseStorage;
        private readonly IMapper _mapper;

        public CartService(IBaseStorage<CartDb> cartStorage,
                           IBaseStorage<UserDb> userStorage,
                           IBaseStorage<CourseDb> courseStorage,
                           IMapper mapper)
        {
            _cartStorage = cartStorage;
            _userStorage = userStorage;
            _courseStorage = courseStorage;
            _mapper = mapper;
        }

        public async Task<BaseResponse<Cart>> AddToCart(string userLogin, Guid courseId)
        {
            try
            {
                // 1. Ищем пользователя
                var users = await _userStorage.GetAllAsync();
                var user = users.FirstOrDefault(x => x.Login == userLogin || x.Email == userLogin);
                if (user == null)
                {
                    return new BaseResponse<Cart>() { Description = "Пользователь не найден", StatusCode = StatusCode.UserNotFound };
                }

                // 2. Ищем курс
                var course = await _courseStorage.GetAsync(courseId);
                if (course == null)
                {
                    return new BaseResponse<Cart>() { Description = "Курс не найден", StatusCode = StatusCode.OK };
                }

                // 3. Проверяем, есть ли уже этот курс в корзине у этого пользователя
                var allCarts = await _cartStorage.GetAllAsync();
                var existingItem = allCarts.FirstOrDefault(x => x.UserId == user.Id && x.CourseId == courseId);

                if (existingItem != null)
                {
                    return new BaseResponse<Cart>() { Description = "Этот курс уже в корзине", StatusCode = StatusCode.OK };
                }

                // 4. Создаем запись
                var newCartItem = new CartDb()
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    CourseId = courseId,
                    DateAdded = DateTime.UtcNow
                };

                await _cartStorage.AddAsync(newCartItem);

                return new BaseResponse<Cart>() { StatusCode = StatusCode.OK, Data = _mapper.Map<Domain.Cart>(newCartItem) };
            }
            catch (Exception ex)
            {
                return new BaseResponse<Cart>() { Description = ex.Message, StatusCode = StatusCode.InternalServerError };
            }
        }

        public async Task<BaseResponse<bool>> DeleteFromCart(Guid id)
        {
            try
            {
                var item = await _cartStorage.GetAsync(id);
                if (item == null)
                {
                    return new BaseResponse<bool>() { Description = "Элемент не найден", StatusCode = StatusCode.OK };
                }

                await _cartStorage.DeleteAsync(item);
                return new BaseResponse<bool>() { Data = true, StatusCode = StatusCode.OK };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>() { Description = ex.Message, StatusCode = StatusCode.InternalServerError };
            }
        }

        public async Task<BaseResponse<List<Domain.Cart>>> GetUserItems(string userLogin)
        {
            try
            {
                var users = await _userStorage.GetAllAsync();
                var user = users.FirstOrDefault(x => x.Login == userLogin || x.Email == userLogin);
                if (user == null)
                {
                    return new BaseResponse<List<Domain.Cart>>() { Description = "Пользователь не найден", StatusCode = StatusCode.UserNotFound };
                }

                var allCarts = await _cartStorage.GetAllAsync();
                // Фильтруем корзину только для текущего пользователя
                var userCarts = allCarts.Where(x => x.UserId == user.Id).ToList();
                // Проверяем, загрузились ли данные о курсе. Если нет - подгружаем вручную.
                // Это нужно, так как generic-репозиторий часто не делает Include.
                foreach (var item in userCarts)
                {
                    if (item.Course == null)
                    {
                        item.Course = await _courseStorage.GetAsync(item.CourseId);
                    }
                }

                var data = _mapper.Map<List<Domain.Cart>>(userCarts);

                return new BaseResponse<List<Domain.Cart>>() { Data = data, StatusCode = StatusCode.OK };
            }
            catch (Exception ex)
            {
                // Теперь мы будем знать, что пошло не так, если ошибка повторится
                return new BaseResponse<List<Domain.Cart>>() { Description = ex.Message, StatusCode = StatusCode.InternalServerError };
            }
        }
    }
}