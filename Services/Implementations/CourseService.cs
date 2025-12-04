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
        public async Task<BaseResponse<List<Course>>> GetCoursesByFilter(CourseFilter filter)
        {
            try
            {
                // 1. Берем все курсы (в реальном проекте лучше фильтровать в SQL, но для учебы пойдет так)
                var allCourses = await _courseStorage.GetAllAsync();

                // 2. Фильтруем по Категории
                var query = allCourses.Where(x => x.CategoryId == filter.CategoryId);

                // 3. Фильтруем по Цене
                query = query.Where(x => x.Price >= filter.MinPrice && x.Price <= filter.MaxPrice);

                // 4. Фильтруем по Уровню (если выбраны галочки)
                if (filter.Levels != null && filter.Levels.Count > 0)
                {
                    // Оставляем только те курсы, уровень которых есть в списке выбранных
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
                        // По умолчанию (например, по имени)
                        query = query.OrderBy(x => x.Name);
                        break;
                }

                var resultList = query.ToList();

                return new BaseResponse<List<Course>>()
                {
                    Data = _mapper.Map<List<Course>>(resultList),
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