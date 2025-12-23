using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace online_courses.Entities
{
    [Table("categories")]
    public class CategoryDb
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("name")]
        public string Name { get; set; }

        [Column("image_path")]
        public string ImagePath { get; set; }

        [Column("course_count")]
        public int CourseCount { get; set; } = 0;

        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Связь с курсами
        public virtual List<CourseDb> Courses { get; set; }
    }
}