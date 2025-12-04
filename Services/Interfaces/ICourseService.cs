using online_courses.Domain;
using online_courses.Models.Filters;
using online_courses.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace online_courses.Services.Interfaces
{
    public interface ICourseService
    {
        Task<BaseResponse<List<Course>>> GetCoursesByCategory(Guid categoryId);

        Task<BaseResponse<List<Course>>> GetCoursesByFilter(CourseFilter filter);

        // НОВЫЙ МЕТОД: Получить один курс по ID
        Task<BaseResponse<Course>> GetCourse(Guid id);
    }
}