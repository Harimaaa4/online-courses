using online_courses.Domain;
using online_courses.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace online_courses.Services.Interfaces
{
    public interface ICartService
    {
        // Получить список товаров пользователя
        Task<BaseResponse<List<Cart>>> GetUserItems(string userLogin);

        // Добавить курс в корзину
        Task<BaseResponse<Cart>> AddToCart(string userLogin, Guid courseId);

        // Удалить курс из корзины
        Task<BaseResponse<bool>> DeleteFromCart(Guid id);
    }
}