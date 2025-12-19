using System;

namespace online_courses.Domain
{
    public class Cart
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid CourseId { get; set; }
        public Course Course { get; set; }
    }
}