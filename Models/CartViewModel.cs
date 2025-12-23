using System;

namespace online_courses.Models
{
    public class CartViewModel
    {
        public Guid Id { get; set; } // ID записи в корзине (для удаления)

        public string CourseName { get; set; }

        public string Description { get; set; } // Добавили описание

        public decimal Price { get; set; }

        public string ImagePath { get; set; }
    }
}