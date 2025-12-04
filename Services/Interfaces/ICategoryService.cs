using online_courses.Domain;
using online_courses.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace online_courses.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<BaseResponse<List<Category>>> GetAllCategories();
    }
}