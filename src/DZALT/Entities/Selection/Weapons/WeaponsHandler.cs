using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Selection.Weapons
{
	public class WeaponsHandler : IRequestHandler<WeaponsQuery, WeaponsResult[]>
	{
		private readonly IRepository repository;

		public WeaponsHandler(IRepository repository)
		{
			this.repository = repository;
		}

		public async Task<WeaponsResult[]> Handle(
			WeaponsQuery query,
			CancellationToken cancellationToken)
		{
			var weapons = await (
				from e in repository.Get<EventLog>()
				where
					(query.From == null || query.From <= e.Date) &&
					(query.To == null || query.To >= e.Date) &&
					e.EnemyPlayer != null &&
					e.Weapon != null &&
					((!query.ExcludeMelee && e.Distance == null) ||
					 (!query.ExcludeDistance && e.Distance != null))
				group new { e.Event } by e.Weapon into g
				select new WeaponsResult()
				{
					Weapon = g.Key,
					Hits = g.Count(x => x.Event == EventLog.EventType.HIT),
					Kills = g.Count(x => x.Event == EventLog.EventType.MURDER),
				}).ToArrayAsync(cancellationToken);

			return weapons;
		}
	}
}
