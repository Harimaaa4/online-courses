using System;

namespace online_courses.Models
{
    public class CourseViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Rating { get; set; }
        public string Level { get; set; }
        public string Image { get; set; }
    }
}