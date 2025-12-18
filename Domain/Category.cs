using online_courses.Entities;
using System;
using System.Collections.Generic;

namespace online_courses.Domain
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public int CourseCount { get; set; }
        public DateTime CreatedDate { get; set; }

        public List<Course> Courses { get; set; }
    }
}