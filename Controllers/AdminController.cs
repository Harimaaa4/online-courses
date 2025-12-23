using AutoMapper; // <--- Добавили
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using online_courses.Entities;
using online_courses.Interfaces;
using System;
using System.Collections.Generic; // Для List
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace online_courses.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IBaseStorage<UserDb> _userStorage;
        private readonly IBaseStorage<CourseDb> _courseStorage;
        private readonly IBaseStorage<CategoryDb> _categoryStorage;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly IMapper _mapper; // <--- Добавили маппер

        public AdminController(IBaseStorage<UserDb> userStorage,
                               IBaseStorage<CourseDb> courseStorage,
                               IBaseStorage<CategoryDb> categoryStorage,
                               IWebHostEnvironment appEnvironment,
                               IMapper mapper) // <--- Внедрили в конструктор
        {
            _userStorage = userStorage;
            _courseStorage = courseStorage;
            _categoryStorage = categoryStorage;
            _appEnvironment = appEnvironment;
            _mapper = mapper;
        }

        public IActionResult Index() => View();

        // ==========================================
        //         УПРАВЛЕНИЕ КАТЕГОРИЯМИ
        // ==========================================

        public async Task<IActionResult> Categories()
        {
            var data = await _categoryStorage.GetAllAsync();

            // ИСПОЛЬЗУЕМ AUTOMAPPER ВМЕСТО РУЧНОГО SELECT
            var model = _mapper.Map<List<online_courses.Models.CategoryViewModel>>(data);

            return View(model);
        }

        // 1. СОЗДАНИЕ КАТЕГОРИИ (GET)
        [HttpGet]
        public IActionResult CreateCategory() => View();

        // 1. СОЗДАНИЕ КАТЕГОРИИ (POST)
        [HttpPost]
        public async Task<IActionResult> CreateCategory(online_courses.Models.CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var allCategories = await _categoryStorage.GetAllAsync();
                if (allCategories.Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("Name", "Такая категория уже существует!");
                    return View(model);
                }

                string imagePath = null;

                if (model.ImageFile != null)
                {
                    string webRootPath = _appEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string folderPath = Path.Combine(webRootPath, "images", "categories");

                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string fullPath = Path.Combine(folderPath, fileName);

                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    imagePath = "/images/categories/" + fileName;
                }

                var newCategory = new online_courses.Entities.CategoryDb
                {
                    Name = model.Name,
                    ImagePath = imagePath
                };

                await _categoryStorage.AddAsync(newCategory);
                return RedirectToAction("Categories");
            }
            return View(model);
        }

        // 2. РЕДАКТИРОВАНИЕ КАТЕГОРИИ (GET)
        [HttpGet]
        public async Task<IActionResult> EditCategory(Guid id)
        {
            var category = await _categoryStorage.GetAsync(id);
            if (category == null) return NotFound();

            // Маппинг одной категории
            var model = _mapper.Map<online_courses.Models.CategoryViewModel>(category);
            return View(model);
        }

        // 2. РЕДАКТИРОВАНИЕ КАТЕГОРИИ (POST)
        [HttpPost]
        public async Task<IActionResult> EditCategory(online_courses.Models.CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = await _categoryStorage.GetAsync(model.Id);
                if (category != null)
                {
                    category.Name = model.Name;

                    if (model.ImageFile != null)
                    {
                        string webRootPath = _appEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        string folderPath = Path.Combine(webRootPath, "images", "categories");

                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                        string fullPath = Path.Combine(folderPath, fileName);

                        using (var fileStream = new FileStream(fullPath, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(fileStream);
                        }

                        category.ImagePath = "/images/categories/" + fileName;
                    }

                    await _categoryStorage.UpdateAsync(category);
                }
                return RedirectToAction("Categories");
            }
            return View(model);
        }

        // 3. УДАЛЕНИЕ КАТЕГОРИИ
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var category = await _categoryStorage.GetAsync(id);
            if (category != null)
            {
                await _categoryStorage.DeleteAsync(category);
            }
            return RedirectToAction("Categories");
        }

        // ==========================================
        //           УПРАВЛЕНИЕ КУРСАМИ
        // ==========================================

        public async Task<IActionResult> Courses()
        {
            var data = await _courseStorage.GetAllAsync();
            // Пока оставляем Entites, так как View настроен на CourseDb
            // Если захотим переделать View на ViewModel, здесь тоже добавим _mapper.Map
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            var categories = await _categoryStorage.GetAllAsync();
            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(online_courses.Models.CourseViewModel model)
        {
            if (ModelState.IsValid)
            {
                string imagePath = null;

                if (model.ImageFile != null)
                {
                    string webRootPath = _appEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string folderPath = Path.Combine(webRootPath, "images", "courses");

                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string fullPath = Path.Combine(folderPath, fileName);

                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    imagePath = "/images/courses/" + fileName;
                }

                var newCourse = new online_courses.Entities.CourseDb
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Author = model.Author,
                    Description = model.Description,
                    Price = model.Price,
                    CategoryId = model.CategoryId,
                    Image = imagePath,
                    CreatedDate = DateTime.UtcNow,
                    Rating = 5,
                    Level = model.Level
                };

                await _courseStorage.AddAsync(newCourse);
                return RedirectToAction("Courses");
            }

            var categories = await _categoryStorage.GetAllAsync();
            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(categories, "Id", "Name");

            return View(model);
        }
        // ==========================================
        //        РЕДАКТИРОВАНИЕ КУРСА
        // ==========================================

        // 2. РЕДАКТИРОВАНИЕ (GET)
        [HttpGet]
        public async Task<IActionResult> EditCourse(Guid id)
        {
            var course = await _courseStorage.GetAsync(id);
            if (course == null) return NotFound();

            // Превращаем БД-объект в Модель (AutoMapper это умеет)
            var model = _mapper.Map<online_courses.Models.CourseViewModel>(course);

            // Загружаем категории для списка
            var categories = await _categoryStorage.GetAllAsync();
            // Важно: 4-й параметр (model.CategoryId) указывает, какая категория выбрана сейчас
            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(categories, "Id", "Name", model.CategoryId);

            return View(model);
        }

        // 2. РЕДАКТИРОВАНИЕ (POST)
        [HttpPost]
        public async Task<IActionResult> EditCourse(online_courses.Models.CourseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var course = await _courseStorage.GetAsync(model.Id);
                if (course != null)
                {
                    // Обновляем данные
                    course.Name = model.Name;
                    course.Author = model.Author;
                    course.Description = model.Description;
                    course.Price = model.Price;
                    course.CategoryId = model.CategoryId;
                    course.Level = model.Level; // Не забываем уровень!

                    // Логика обновления картинки
                    if (model.ImageFile != null)
                    {
                        string webRootPath = _appEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        string folderPath = Path.Combine(webRootPath, "images", "courses");

                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                        string fullPath = Path.Combine(folderPath, fileName);

                        using (var fileStream = new FileStream(fullPath, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(fileStream);
                        }

                        course.Image = "/images/courses/" + fileName;
                    }

                    await _courseStorage.UpdateAsync(course);
                }
                return RedirectToAction("Courses");
            }

            // Если ошибка, восстанавливаем список категорий
            var categories = await _categoryStorage.GetAllAsync();
            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(categories, "Id", "Name", model.CategoryId);

            return View(model);
        }

        //          УДАЛЕНИЕ КУРСА
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var course = await _courseStorage.GetAsync(id);
            if (course != null)
            {
                await _courseStorage.DeleteAsync(course);
            }
            return RedirectToAction("Courses");
        }
    }
}