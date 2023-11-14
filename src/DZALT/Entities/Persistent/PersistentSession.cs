using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DZALT.Entities.Mapping;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Persistent
{
	public class PersistentSession : DbContext, ISession
	{
		private Action<DbContextOptionsBuilder> config;
		
		public PersistentSession(Action<DbContextOptionsBuilder> config)
			: base()
		{
			this.config = config;
			Database.EnsureCreated();
		}

		IQueryable<T> IRepository.Get<T>()
		{
			return Set<T>().AsNoTracking();
		}

		IQueryable<T> IRepository.GetUpdatable<T>()
		{
			return Set<T>();
		}

		void IRepository.Add<T>(T entity)
		{
			Set<T>().Add(entity);
		}

		void IRepository.AddRange<T>(IEnumerable<T> entities)
		{
			if (!entities.Any())
			{
				return;
			}
			Set<T>().AddRange(entities);
		}

		void IRepository.Remove<T>(T entity)
		{
			Set<T>().Remove(entity);
		}

		void IRepository.RemoveRange<T>(IEnumerable<T> entities)
		{
			if (!entities.Any())
			{
				return;
			}
			Set<T>().RemoveRange(entities);
		}

		async Task ISession.SubmitChanges(CancellationToken cancellationToken)
		{
			await SaveChangesAsync(cancellationToken);
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			config(optionsBuilder);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new PlayerMapping());
			modelBuilder.ApplyConfiguration(new NicknameMapping());
			modelBuilder.ApplyConfiguration(new SessionLogMapping());
		}
	}
}
