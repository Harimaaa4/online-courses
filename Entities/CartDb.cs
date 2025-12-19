#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace online_courses.Entities
{
    [Table("carts")] // Назовем таблицу carts
    public class CartDb
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("user_id")]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserDb? User { get; set; }

        [Column("course_id")]
        public Guid CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual CourseDb? Course { get; set; }

        [Column("date_added")]
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
}