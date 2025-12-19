#nullable enable 
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace online_courses.Entities
{
    [Table("users")]
    public class UserDb
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("login")]
        public string Login { get; set; } = null!;

        [Column("password")]
        public string Password { get; set; } = null!;

        [Column("email")]
        public string Email { get; set; } = null!;

        [Column("role")]
        public string Role { get; set; } = null!;

        [Column("image_path")]
        public string? ImagePath { get; set; } // Вопросительный знак значит "может быть пустым"

        [Column("full_name")]
        public string? FullName { get; set; } // ФИО (может быть пустым)

        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}