using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UserManagement.DataAccess.Data;
using UserManagement.Domain.Repository;

namespace UserMan.DataAccess.Implementation
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public void Add(T entity)
        {
            _context.Set<T>().Add(entity);
            Save();
        }

        public void AddRange(IEnumerable<T> entities)
        {
            _context.Set<T>().AddRange(entities);
            Save();
        }

        public void Delete(int id)
        {
            T entity = _context.Set<T>().Find(id);
            _context.Set<T>().Remove(entity);
            Save();
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().Where(predicate);
        }

        public T Get(int id)
        {
            return _context.Set<T>().Find(id);
        }

        public IEnumerable<T> GetAll()
        {
           return  _context.Set<T>().ToList();
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);   
            Save();
        }

        public void Save()
        { 
            _context.SaveChanges();
        }
    }
}
