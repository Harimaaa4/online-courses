using online_courses.Domain;
using online_courses.Response;
using online_courses.Models;
using System.Threading.Tasks;
using System.Security.Claims;

namespace online_courses.Services.Interfaces
{
    public interface IAccountService
    {
        Task<BaseResponse<User>> Register(RegisterViewModel model);

        Task<BaseResponse<ClaimsIdentity>> Login(LoginViewModel model);

        Task<BaseResponse<ClaimsIdentity>> ConfirmEmail(User user, string code);
        
        Task<BaseResponse<ClaimsIdentity>> IsCreatedAccount(User model);
    }
}