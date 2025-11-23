using System;

namespace online_courses.Domain
{
    public class Course
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } // Сама категория (объект)

        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Rating { get; set; }
        public string Level { get; set; }
        public string Image { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}