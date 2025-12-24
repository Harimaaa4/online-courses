using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using online_courses.Entities;
using online_courses.Interfaces;
using online_courses.Models; // Добавил для использования ViewModel
using System;
using System.Collections.Generic;
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
        private readonly IBaseStorage<OrderDb> _orderStorage; // Добавили для статистики
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly IMapper _mapper;

        public AdminController(IBaseStorage<UserDb> userStorage,
                               IBaseStorage<CourseDb> courseStorage,
                               IBaseStorage<CategoryDb> categoryStorage,
                               IBaseStorage<OrderDb> orderStorage,
                               IWebHostEnvironment appEnvironment,
                               IMapper mapper)
        {
            _userStorage = userStorage;
            _courseStorage = courseStorage;
            _categoryStorage = categoryStorage;
            _orderStorage = orderStorage;
            _appEnvironment = appEnvironment;
            _mapper = mapper;
        }

        // === ГЛАВНАЯ (ДАШБОРД) ===
        public async Task<IActionResult> Index()
        {
            var users = await _userStorage.GetAllAsync();
            var courses = await _courseStorage.GetAllAsync();
            var orders = await _orderStorage.GetAllAsync();

            var model = new AdminDashboardViewModel
            {
                UsersCount = users.Count,
                CoursesCount = courses.Count,
                OrdersToday = orders.Count(x => x.CreatedDate.Date == DateTime.UtcNow.Date),
                RevenueTotal = orders.Sum(x => x.TotalPrice)
            };

            return View(model);
        }

        // ==========================================
        //         УПРАВЛЕНИЕ КАТЕГОРИЯМИ
        // ==========================================

        public async Task<IActionResult> Categories(string searchString)
        {
            var data = await _categoryStorage.GetAllAsync();

            // === ПОИСК ПО КАТЕГОРИЯМ ===
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                data = data.Where(c => c.Name.ToLower().Contains(searchString)).ToList();
            }
            ViewBag.CurrentFilter = searchString;
            // ============================

            var model = _mapper.Map<List<CategoryViewModel>>(data);
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateCategory() => View();

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryViewModel model)
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
                    string folderPath = Path.Combine(_appEnvironment.WebRootPath, "images", "categories");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    using (var fileStream = new FileStream(Path.Combine(folderPath, fileName), FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }
                    imagePath = "/images/categories/" + fileName;
                }

                var newCategory = new CategoryDb { Name = model.Name, ImagePath = imagePath };
                await _categoryStorage.AddAsync(newCategory);
                return RedirectToAction("Categories");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(Guid id)
        {
            var category = await _categoryStorage.GetAsync(id);
            if (category == null) return NotFound();
            return View(_mapper.Map<CategoryViewModel>(category));
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = await _categoryStorage.GetAsync(model.Id);
                if (category != null)
                {
                    category.Name = model.Name;
                    if (model.ImageFile != null)
                    {
                        string folderPath = Path.Combine(_appEnvironment.WebRootPath, "images", "categories");
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                        using (var fileStream = new FileStream(Path.Combine(folderPath, fileName), FileMode.Create))
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

        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var category = await _categoryStorage.GetAsync(id);
            if (category != null) await _categoryStorage.DeleteAsync(category);
            return RedirectToAction("Categories");
        }

        // ==========================================
        //           УПРАВЛЕНИЕ КУРСАМИ
        // ==========================================

        public async Task<IActionResult> Courses(string searchString)
        {
            var data = await _courseStorage.GetAllAsync();

            // === ПОИСК ПО КУРСАМ ===
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                // Ищем по Названию ИЛИ по Автору
                data = data.Where(c =>
                    (c.Name != null && c.Name.ToLower().Contains(searchString)) ||
                    (c.Author != null && c.Author.ToLower().Contains(searchString))
                ).ToList();
            }
            ViewBag.CurrentFilter = searchString;
            // =======================

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
        public async Task<IActionResult> CreateCourse(CourseViewModel model)
        {
            if (ModelState.IsValid)
            {
                string imagePath = null;
                if (model.ImageFile != null)
                {
                    string folderPath = Path.Combine(_appEnvironment.WebRootPath, "images", "courses");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    using (var fileStream = new FileStream(Path.Combine(folderPath, fileName), FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }
                    imagePath = "/images/courses/" + fileName;
                }

                var newCourse = new CourseDb
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

        [HttpGet]
        public async Task<IActionResult> EditCourse(Guid id)
        {
            var course = await _courseStorage.GetAsync(id);
            if (course == null) return NotFound();
            var model = _mapper.Map<CourseViewModel>(course);
            var categories = await _categoryStorage.GetAllAsync();
            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditCourse(CourseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var course = await _courseStorage.GetAsync(model.Id);
                if (course != null)
                {
                    course.Name = model.Name;
                    course.Author = model.Author;
                    course.Description = model.Description;
                    course.Price = model.Price;
                    course.CategoryId = model.CategoryId;
                    course.Level = model.Level;

                    if (model.ImageFile != null)
                    {
                        string folderPath = Path.Combine(_appEnvironment.WebRootPath, "images", "courses");
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                        using (var fileStream = new FileStream(Path.Combine(folderPath, fileName), FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(fileStream);
                        }
                        course.Image = "/images/courses/" + fileName;
                    }
                    await _courseStorage.UpdateAsync(course);
                }
                return RedirectToAction("Courses");
            }
            var categories = await _categoryStorage.GetAllAsync();
            ViewBag.Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var course = await _courseStorage.GetAsync(id);
            if (course != null) await _courseStorage.DeleteAsync(course);
            return RedirectToAction("Courses");
        }

        // ==========================================
        //        УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ
        // ==========================================

        public async Task<IActionResult> Users(string searchString)
        {
            var users = await _userStorage.GetAllAsync();
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                users = users.Where(u =>
                    (u.Login != null && u.Login.ToLower().Contains(searchString)) ||
                    (u.Email != null && u.Email.ToLower().Contains(searchString))
                ).ToList();
            }
            ViewBag.CurrentFilter = searchString;
            return View(users);
        }

        public async Task<IActionResult> ToggleRole(Guid id)
        {
            var user = await _userStorage.GetAsync(id);
            if (user != null)
            {
                if (User.Identity.Name == user.Login)
                {
                    TempData["Error"] = "Вы не можете изменить роль самому себе!";
                    return RedirectToAction("Users");
                }
                user.Role = (user.Role == "Admin") ? "User" : "Admin";
                await _userStorage.UpdateAsync(user);
            }
            return RedirectToAction("Users");
        }

        // ==========================================
        //           УПРАВЛЕНИЕ ЗАКАЗАМИ
        // ==========================================
        public async Task<IActionResult> Orders()
        {
            var orders = await _orderStorage.GetAllAsync();
            return View(orders.OrderByDescending(x => x.CreatedDate).ToList());
        }
    }
}