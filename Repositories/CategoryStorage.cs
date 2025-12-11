using Microsoft.EntityFrameworkCore;
using online_courses.Data;
using online_courses.Entities;
using online_courses.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace online_courses.Repositories
{
    public class CategoryStorage : IBaseStorage<CategoryDb>
    {
        private readonly ApplicationDbContext _db;

        public CategoryStorage(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(CategoryDb entity)
        {
            await _db.Categories.AddAsync(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(CategoryDb entity)
        {
            _db.Categories.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<List<CategoryDb>> GetAllAsync()
        {
            return await _db.Categories
                .Include(x => x.Courses)
                .ToListAsync();
        }

        public async Task<CategoryDb> GetAsync(Guid id)
        {
            return await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task UpdateAsync(CategoryDb entity)
        {
            _db.Categories.Update(entity);
            await _db.SaveChangesAsync();
        }
    }
}