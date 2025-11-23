using Microsoft.EntityFrameworkCore;
using online_courses.Data;
using online_courses.Entities;
using online_courses.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace online_courses.Repositories
{
    public class CourseStorage : IBaseStorage<CourseDb>
    {
        private readonly ApplicationDbContext _db;

        public CourseStorage(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(CourseDb entity)
        {
            await _db.Courses.AddAsync(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(CourseDb entity)
        {
            _db.Courses.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<List<CourseDb>> GetAllAsync()
        {
            // Подгружаем категорию вместе с курсом
            return await _db.Courses.Include(x => x.Category).ToListAsync();
        }

        public async Task<CourseDb> GetAsync(Guid id)
        {
            return await _db.Courses
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task UpdateAsync(CourseDb entity)
        {
            _db.Courses.Update(entity);
            await _db.SaveChangesAsync();
        }
    }
}