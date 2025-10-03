// Repositories/Interfaces/IRepository.cs
using System.Collections.Generic;

namespace ReolMarked.MVVM.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        T Add(T entity);
        T GetById(int id);
        IEnumerable<T> GetAll();
        void Update(T entity);
        void Delete(int id);
    }
}