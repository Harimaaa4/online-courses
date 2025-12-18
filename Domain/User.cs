using System;

namespace online_courses.Domain
{
    public class User
    {
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedDate { get; set; }

        public string GeneratedCode { get; set; }
    }
}