using online_courses.Entities;
using online_courses.Interfaces;
using online_courses.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace online_courses.Services.Implementations
{
    public class Initializer : IInitializer
    {
        private readonly IBaseStorage<CategoryDb> _categoryStorage;
        private readonly IBaseStorage<CourseDb> _courseStorage; // Добавили хранилище курсов

        public Initializer(IBaseStorage<CategoryDb> categoryStorage, IBaseStorage<CourseDb> courseStorage)
        {
            _categoryStorage = categoryStorage;
            _courseStorage = courseStorage;
        }

        public async Task Initialize()
        {
            var categories = await _categoryStorage.GetAllAsync();

            // 1. Создаем категории, если их нет
            if (categories.Count == 0)
            {
                await _categoryStorage.AddAsync(new CategoryDb { Id = Guid.NewGuid(), Name = "Программирование", Image = "~/images/prog.jpg", CreatedDate = DateTime.UtcNow });
                await _categoryStorage.AddAsync(new CategoryDb { Id = Guid.NewGuid(), Name = "Дизайн", Image = "~/images/design.jpg", CreatedDate = DateTime.UtcNow });
                await _categoryStorage.AddAsync(new CategoryDb { Id = Guid.NewGuid(), Name = "Маркетинг", Image = "~/images/marketing.jpg", CreatedDate = DateTime.UtcNow });

                // Обновляем список, чтобы получить ID только что созданных категорий
                categories = await _categoryStorage.GetAllAsync();
            }

            // 2. Создаем курсы, если их нет
            var courses = await _courseStorage.GetAllAsync();
            if (courses.Count == 0)
            {
                // Находим ID категорий
                var progId = categories.FirstOrDefault(x => x.Name == "Программирование")?.Id ?? Guid.Empty;
                var designId = categories.FirstOrDefault(x => x.Name == "Дизайн")?.Id ?? Guid.Empty;

                if (progId != Guid.Empty)
                {
                    await _courseStorage.AddAsync(new CourseDb
                    {
                        Id = Guid.NewGuid(),
                        CategoryId = progId,
                        Name = "Основы C#",
                        Author = "Microsoft",
                        Description = "Изучите основы языка C# с нуля.",
                        Price = 1500,
                        Rating = 5,
                        Level = "Beginner",
                        Image = "~/images/csharp.jpg",
                        CreatedDate = DateTime.UtcNow
                    });

                    await _courseStorage.AddAsync(new CourseDb
                    {
                        Id = Guid.NewGuid(),
                        CategoryId = progId,
                        Name = "Python для Data Science",
                        Author = "Google",
                        Description = "Анализ данных на Python.",
                        Price = 2000,
                        Rating = 4,
                        Level = "Middle",
                        Image = "~/images/python.png",
                        CreatedDate = DateTime.UtcNow
                    });
                }

                if (designId != Guid.Empty)
                {
                    await _courseStorage.AddAsync(new CourseDb
                    {
                        Id = Guid.NewGuid(),
                        CategoryId = designId,
                        Name = "Figma Мастер",
                        Author = "Adobe",
                        Description = "Создание интерфейсов в Figma.",
                        Price = 1200,
                        Rating = 5,
                        Level = "Beginner",
                        Image = "~/images/photoshop.png",
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }
        }
    }
}