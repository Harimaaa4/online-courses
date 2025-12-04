using online_courses.Domain;
using online_courses.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace online_courses.Services.Interfaces
{
    public interface ICourseService
    {
        // Метод для получения курсов конкретной категории
        Task<BaseResponse<List<Course>>> GetCoursesByCategory(Guid categoryId);
    }
}