using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace online_courses.Interfaces
{
    public interface IBaseStorage<T>
    {
        Task AddAsync(T entity);        // Создать
        Task DeleteAsync(T entity);     // Удалить
        Task<T> GetAsync(Guid id);      // Получить одного
        Task<List<T>> GetAllAsync();    // Получить список
        Task UpdateAsync(T entity);     // Обновить
    }
}