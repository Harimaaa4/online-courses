using AutoMapper;
using online_courses.Domain;
using online_courses.Entities;
using online_courses.Interfaces;
using online_courses.Models.Filters;
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

        public async Task<BaseResponse<List<Course>>> GetCoursesByFilter(CourseFilter filter)
        {
            try
            {
                var allCourses = await _courseStorage.GetAllAsync();

                // 1. Фильтр по категории
                var query = allCourses.Where(x => x.CategoryId == filter.CategoryId);

                // 2. Фильтр по цене
                query = query.Where(x => x.Price >= filter.MinPrice && x.Price <= filter.MaxPrice);

                // 3. НОВОЕ: Поиск по названию (Глава 25)
                if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
                {
                    query = query.Where(x => x.Name.Contains(filter.SearchQuery, StringComparison.OrdinalIgnoreCase));
                }

                // 4. Фильтр по уровню
                if (filter.Levels != null && filter.Levels.Count > 0)
                {
                    query = query.Where(x => filter.Levels.Contains(x.Level));
                }

                // 5. Сортировка
                switch (filter.SortType)
                {
                    case "price_asc":
                        query = query.OrderBy(x => x.Price);
                        break;
                    case "price_desc":
                        query = query.OrderByDescending(x => x.Price);
                        break;
                    case "rating":
                        query = query.OrderByDescending(x => x.Rating);
                        break;
                    default:
                        query = query.OrderBy(x => x.Name);
                        break;
                }

                return new BaseResponse<List<Course>>()
                {
                    Data = _mapper.Map<List<Course>>(query.ToList()),
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

        public async Task<BaseResponse<Course>> GetCourse(Guid id)
        {
            try
            {
                var course = await _courseStorage.GetAsync(id);
                if (course == null)
                {
                    return new BaseResponse<Course>()
                    {
                        Description = "Курс не найден",
                        StatusCode = StatusCode.UserNotFound
                    };
                }
                return new BaseResponse<Course>()
                {
                    Data = _mapper.Map<Course>(course),
                    StatusCode = StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<Course>()
                {
                    Description = ex.Message,
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }
    }
}