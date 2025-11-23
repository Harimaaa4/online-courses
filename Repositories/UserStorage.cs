using Microsoft.EntityFrameworkCore;
using online_courses.Data;
using online_courses.Entities;
using online_courses.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace online_courses.Repositories
{
    public class UserStorage : IBaseStorage<UserDb>
    {
        private readonly ApplicationDbContext _db;

        public UserStorage(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(UserDb entity)
        {
            await _db.Users.AddAsync(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(UserDb entity)
        {
            _db.Users.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<List<UserDb>> GetAllAsync()
        {
            return await _db.Users.ToListAsync();
        }

        public async Task<UserDb> GetAsync(Guid id)
        {
            return await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task UpdateAsync(UserDb entity)
        {
            _db.Users.Update(entity);
            await _db.SaveChangesAsync();
        }
    }
}