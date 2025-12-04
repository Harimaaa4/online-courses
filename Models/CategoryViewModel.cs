using System;

namespace online_courses.Models
{
    public class CategoryViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public int CourseCount { get; set; }
    }
}