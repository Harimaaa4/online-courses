using AutoMapper;
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
    public class CourseService : ICourseService
    {
        private readonly IBaseStorage<CourseDb> _courseStorage;
        private readonly IMapper _mapper;

        public CourseService(IBaseStorage<CourseDb> courseStorage, IMapper mapper)
        {
            _courseStorage = courseStorage;
            _mapper = mapper;
        }

        public async Task<BaseResponse<List<Course>>> GetCoursesByCategory(Guid categoryId)
        {
            try
            {
                var allCourses = await _courseStorage.GetAllAsync();
                // Фильтруем: берем только те, у которых CategoryId совпадает
                var courses = allCourses.Where(x => x.CategoryId == categoryId).ToList();

                return new BaseResponse<List<Course>>()
                {
                    Data = _mapper.Map<List<Course>>(courses),
                    StatusCode = StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<Course>>()
                {
                    Description = ex.Message,
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }
    }
}