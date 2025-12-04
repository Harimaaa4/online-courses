using System;
using System.Collections.Generic;

namespace online_courses.Models.Filters
{
    public class CourseFilter
    {
        public Guid CategoryId { get; set; } // Чтобы искать только в текущей категории
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public List<string> Levels { get; set; } // Список выбранных уровней (Beginner, Middle...)

        // Для сортировки
        public string SortType { get; set; } // "price_asc", "rating", и т.д.
    }
}