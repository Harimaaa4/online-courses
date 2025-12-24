using online_courses.Data;
using online_courses.Entities;
using online_courses.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace online_courses.Repositories
{
    public class OrderStorage : IBaseStorage<OrderDb>
    {
        private readonly ApplicationDbContext _db;

        public OrderStorage(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(OrderDb item)
        {
            await _db.Orders.AddAsync(item);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(OrderDb item)
        {
            _db.Orders.Remove(item);
            await _db.SaveChangesAsync();
        }

        public async Task<List<OrderDb>> GetAllAsync()
        {
            return await _db.Orders.ToListAsync();
        }

        public async Task<OrderDb> GetAsync(Guid id)
        {
            return await _db.Orders.FirstOrDefaultAsync(x => x.Id == id);
        }

        // === ИСПРАВЛЕНИЕ ===
        // Убрали <OrderDb> из возвращаемого типа и return item
        public async Task UpdateAsync(OrderDb item)
        {
            _db.Orders.Update(item);
            await _db.SaveChangesAsync();
        }
    }
}