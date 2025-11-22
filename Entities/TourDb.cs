using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace online_courses.Entities
{
    [Table("tours")]
    public class TourDb
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("country_id")]
        public Guid CountryId { get; set; }

        [ForeignKey("CountryId")]
        public CountryDb Country { get; set; }

        [Column("city")]
        public string City { get; set; }

        [Column("hotel_name")]
        public string HotelName { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("price_adult")]
        public decimal PriceAdult { get; set; }

        [Column("price_child")]
        public decimal PriceChild { get; set; }

        [Column("stars")]
        public int Stars { get; set; }

        [Column("food")]
        public string Food { get; set; }

        [Column("image")]
        public string Image { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}