namespace online_courses.Models
{
    public class AdminDashboardViewModel
    {
        public int UsersCount { get; set; }
        public int CoursesCount { get; set; }
        public int OrdersToday { get; set; }
        public decimal RevenueTotal { get; set; } // Выручка
    }
}