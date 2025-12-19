using Microsoft.EntityFrameworkCore;
using online_courses.Data;
using online_courses.Entities;
using online_courses.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace online_courses.Repositories
{
    public class CartStorage : IBaseStorage<CartDb>
    {
        private readonly ApplicationDbContext _db;

        public CartStorage(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(CartDb entity)
        {
            await _db.Carts.AddAsync(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(CartDb entity)
        {
            _db.Carts.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<List<CartDb>> GetAllAsync()
        {
            // Важно: используем .Include, чтобы сразу подтянуть информацию о Курсе (название, цену, картинку)
            return await _db.Carts
                .Include(x => x.Course)
                .ToListAsync();
        }

        public async Task<CartDb> GetAsync(Guid id)
        {
            return await _db.Carts
                .Include(x => x.Course)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task UpdateAsync(CartDb entity)
        {
            _db.Carts.Update(entity);
            await _db.SaveChangesAsync();
        }
    }
}