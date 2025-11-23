using online_courses.Domain;
using online_courses.Response;
using online_courses.Models;
using System.Threading.Tasks;
using System.Security.Claims;

namespace online_courses.Services.Interfaces
{
    public interface IAccountService
    {
        Task<BaseResponse<ClaimsIdentity>> Register(RegisterViewModel model);
        Task<BaseResponse<ClaimsIdentity>> Login(LoginViewModel model);
    }
}