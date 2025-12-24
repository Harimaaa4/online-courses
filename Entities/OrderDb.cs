using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace online_courses.Entities
{
    [Table("orders")]
    public class OrderDb
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("user_email")]
        public string UserEmail { get; set; } // Сохраним почту на случай, если юзер удалится

        [Column("course_names")]
        public string CourseNames { get; set; } // Список курсов через запятую

        [Column("total_price")]
        public decimal TotalPrice { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Column("status")]
        public string Status { get; set; } // "Создан", "Оплачен", "Отменен"
    }
}