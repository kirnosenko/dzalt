using System.Collections.Generic;
using System.Linq;

namespace DZALT
{
	public interface IRepository
	{
		IQueryable<T> Get<T>() where T : class;
		IQueryable<T> GetUpdatable<T>() where T : class;
		void Add<T>(T entity) where T : class;
		void AddRange<T>(IEnumerable<T> entities) where T : class;
		void Remove<T>(T entity) where T : class;
		void RemoveRange<T>(IEnumerable<T> entities) where T : class;
	}
}
