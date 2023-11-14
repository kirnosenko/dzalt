using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Persistent
{
	public class InMemoryDataStore : NamedDataStore
	{
		public InMemoryDataStore(string name)
			: base(name)
		{
		}

		protected override void Configure(DbContextOptionsBuilder options)
		{
			options.UseInMemoryDatabase(name, o => o.EnableNullChecks(false));
		}
	}
}
