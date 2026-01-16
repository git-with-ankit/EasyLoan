using EasyLoan.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.DataAccess.Interfaces
{
    public interface IRepository<T> where T : class
    {
        public Task<T?> GetByIdAsync(Guid id);
        public Task<List<T>> GetAllAsync();
        Task AddAsync(T entity);
        public Task SaveChangesAsync();
    }
}
//can add update
//Dtos review
//IRepository to keep or not