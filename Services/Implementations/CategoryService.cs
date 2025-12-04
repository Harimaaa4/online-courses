using AutoMapper;
using online_courses.Domain;
using online_courses.Entities;
using online_courses.Interfaces;
using online_courses.Response;
using online_courses.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace online_courses.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly IBaseStorage<CategoryDb> _categoryStorage;
        private readonly IMapper _mapper;

        public CategoryService(IBaseStorage<CategoryDb> categoryStorage, IMapper mapper)
        {
            _categoryStorage = categoryStorage;
            _mapper = mapper;
        }

        public async Task<BaseResponse<List<Category>>> GetAllCategories()
        {
            try
            {
                var categoriesDb = await _categoryStorage.GetAllAsync();
                var categories = _mapper.Map<List<Category>>(categoriesDb);

                return new BaseResponse<List<Category>>()
                {
                    Data = categories,
                    StatusCode = StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<Category>>()
                {
                    Description = ex.Message,
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }
    }
}