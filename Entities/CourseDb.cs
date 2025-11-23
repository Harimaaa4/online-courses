using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace online_courses.Entities
{
    [Table("courses")] // Таблица courses
    public class CourseDb
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("category_id")]
        public Guid CategoryId { get; set; } // Ссылка на категорию

        [ForeignKey("CategoryId")]
        public CategoryDb Category { get; set; }

        [Column("name")]
        public string Name { get; set; } // Название курса (бывший HotelName)

        [Column("author")]
        public string Author { get; set; } // Автор курса (бывший City)

        [Column("description")]
        public string Description { get; set; }

        [Column("price")]
        public decimal Price { get; set; } // Цена

        [Column("rating")]
        public int Rating { get; set; } // Рейтинг (бывшие Stars)

        [Column("level")]
        public string Level { get; set; } // Уровень сложности (бывший Food, например: "Junior", "Middle")

        [Column("image")]
        public string Image { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}