using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Domain.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T Get(string id);
        IEnumerable<T> Find(Expression<Func<T,bool>> predicate);
        void Add(T entity); 
        void AddRange(IEnumerable<T> entities);
        void Update(T entity);
        void Delete(string id);
        void Save();
    }
}
