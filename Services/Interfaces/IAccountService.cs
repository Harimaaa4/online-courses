using online_courses.Domain;
using online_courses.Response;
using online_courses.Models;
using System.Threading.Tasks;
using System.Security.Claims;

namespace online_courses.Services.Interfaces
{
    public interface IAccountService
    {
        // Register теперь возвращает User (с кодом), а не ClaimsIdentity (вход)
        Task<BaseResponse<User>> Register(RegisterViewModel model);

        Task<BaseResponse<ClaimsIdentity>> Login(LoginViewModel model);

        // Новый метод для финального подтверждения
        Task<BaseResponse<ClaimsIdentity>> ConfirmEmail(User user, string code);
    }
}