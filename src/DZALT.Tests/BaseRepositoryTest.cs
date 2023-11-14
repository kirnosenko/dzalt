using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using DZALT.Entities.Persistent;

namespace DZALT
{
	public abstract class BaseRepositoryTest : ISession
	{
		private InMemoryDataStore data;
		private ISession session;

		public BaseRepositoryTest()
		{
			data = new InMemoryDataStore(Guid.NewGuid().ToString());
			session = data.OpenSession();
		}

		public void Dispose()
		{
			session.Dispose();
		}

		public IQueryable<T> Get<T>() where T : class
		{
			return session.Get<T>();
		}

		public IQueryable<T> GetUpdatable<T>() where T : class
		{
			return session.Get<T>();
		}

		public void Add<T>(T entity) where T : class
		{
			session.Add(entity);
		}

		public void AddRange<T>(IEnumerable<T> entities) where T : class
		{
			session.AddRange(entities);
		}

		public void Remove<T>(T entity) where T : class
		{
			session.Remove(entity);
		}

		public void RemoveRange<T>(IEnumerable<T> entities) where T : class
		{
			session.RemoveRange(entities);
		}

		public async Task SubmitChanges(CancellationToken cancellationToken = default)
		{
			await session.SubmitChanges(cancellationToken);
		}
	}
}
