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
        private readonly IBaseStorage<CourseDb> _courseStorage;

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
                await _categoryStorage.AddAsync(new CategoryDb { Id = Guid.NewGuid(), Name = "Программирование", ImagePath = "/images/prog.jpg", CreatedDate = DateTime.UtcNow });
                await _categoryStorage.AddAsync(new CategoryDb { Id = Guid.NewGuid(), Name = "Дизайн", ImagePath = "/images/design.jpg", CreatedDate = DateTime.UtcNow });
                await _categoryStorage.AddAsync(new CategoryDb { Id = Guid.NewGuid(), Name = "Маркетинг", ImagePath = "/images/marketing.jpg", CreatedDate = DateTime.UtcNow });

                categories = await _categoryStorage.GetAllAsync();
            }

            // 2. Создаем курсы, если их нет
            var courses = await _courseStorage.GetAllAsync();
            if (courses.Count == 0)
            {
                var progId = categories.FirstOrDefault(x => x.Name == "Программирование")?.Id ?? Guid.Empty;
                var designId = categories.FirstOrDefault(x => x.Name == "Дизайн")?.Id ?? Guid.Empty;
                var marketId = categories.FirstOrDefault(x => x.Name == "Маркетинг")?.Id ?? Guid.Empty;

                // --- ПРОГРАММИРОВАНИЕ ---
                if (progId != Guid.Empty)
                {
                    await _courseStorage.AddAsync(new CourseDb
                    {
                        Id = Guid.NewGuid(),
                        CategoryId = progId,
                        Name = "Основы C#",
                        Author = "Microsoft",
                        Description = "Изучите основы языка C# с нуля. Переменные, циклы, ООП.",
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
                        Description = "Анализ данных, Pandas, NumPy и машинное обучение.",
                        Price = 2000,
                        Rating = 4,
                        Level = "Middle",
                        Image = "~/images/python.png",
                        CreatedDate = DateTime.UtcNow
                    });

                    await _courseStorage.AddAsync(new CourseDb
                    {
                        Id = Guid.NewGuid(),
                        CategoryId = progId,
                        Name = "Java Разработчик",
                        Author = "Oracle",
                        Description = "Создание мощных Enterprise приложений на Java.",
                        Price = 2500,
                        Rating = 5,
                        Level = "Senior",
                        Image = "~/images/java.png",
                        CreatedDate = DateTime.UtcNow
                    });
                }

                // --- ДИЗАЙН ---
                if (designId != Guid.Empty)
                {
                    await _courseStorage.AddAsync(new CourseDb
                    {
                        Id = Guid.NewGuid(),
                        CategoryId = designId,
                        Name = "Figma Мастер",
                        Author = "Adobe",
                        Description = "Создание интерфейсов сайтов и мобильных приложений.",
                        Price = 1200,
                        Rating = 5,
                        Level = "Beginner",
                        Image = "~/images/figma.jpg",
                        CreatedDate = DateTime.UtcNow
                    });

                    await _courseStorage.AddAsync(new CourseDb
                    {
                        Id = Guid.NewGuid(),
                        CategoryId = designId,
                        Name = "UX/UI Дизайн Про",
                        Author = "Skillbox",
                        Description = "Проектирование удобных пользовательских интерфейсов.",
                        Price = 1800,
                        Rating = 4,
                        Level = "Middle",
                        Image = "~/images/ui_ux.jpg",
                        CreatedDate = DateTime.UtcNow
                    });
                }

                // --- МАРКЕТИНГ ---
                if (marketId != Guid.Empty)
                {
                    await _courseStorage.AddAsync(new CourseDb
                    {
                        Id = Guid.NewGuid(),
                        CategoryId = marketId,
                        Name = "SMM-менеджер",
                        Author = "Instagram",
                        Description = "Продвижение брендов в социальных сетях. Таргет и контент.",
                        Price = 1400,
                        Rating = 5,
                        Level = "Beginner",
                        Image = "~/images/smm_менеджер.jpg",
                        CreatedDate = DateTime.UtcNow
                    });

                    await _courseStorage.AddAsync(new CourseDb
                    {
                        Id = Guid.NewGuid(),
                        CategoryId = marketId,
                        Name = "Интернет-маркетолог",
                        Author = "Yandex",
                        Description = "Комплексный маркетинг: SEO, контекстная реклама, аналитика.",
                        Price = 2200,
                        Rating = 5,
                        Level = "Middle",
                        Image = "~/images/marketing.jpg",
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }
        }
    }
}