using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace online_courses.Entities
{
    [Table("categories")] // Таблица в базе будет называться categories
    public class CategoryDb
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("name")]
        public string Name { get; set; } // Название категории

        [Column("image")]
        public string Image { get; set; }

        [Column("course_count")]
        public int CourseCount { get; set; } // Количество курсов в категории

        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public List<CourseDb> Courses { get; set; } // Связь: Одна категория -> Много курсов
    }
}