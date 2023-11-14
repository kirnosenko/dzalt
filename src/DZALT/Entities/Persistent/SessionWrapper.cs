using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DZALT.Entities.Persistent
{
	public class SessionWrapper : ISession
	{
		private ISession innerSession;

		public SessionWrapper(ISession innerSession)
		{
			this.innerSession = innerSession;
		}

		public void Dispose()
		{
			innerSession = null;
		}

		public IQueryable<T> Get<T>() where T : class
		{
			return innerSession.Get<T>();
		}

		public IQueryable<T> GetUpdatable<T>() where T : class
		{
			return innerSession.GetUpdatable<T>();
		}

		public void Add<T>(T entity) where T : class
		{
			innerSession.Add(entity);
		}

		public void AddRange<T>(IEnumerable<T> entities) where T : class
		{
			innerSession.AddRange(entities);
		}

		public void Remove<T>(T entity) where T : class
		{
			innerSession.Remove(entity);
		}

		public void RemoveRange<T>(IEnumerable<T> entities) where T : class
		{
			innerSession.RemoveRange(entities);
		}

		public async Task SubmitChanges(CancellationToken cancellationToken)
		{
			await innerSession.SubmitChanges(cancellationToken);
		}
	}
}
