using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Selection.MultipleHits
{
	public class MultipleHitsHandler : IRequestHandler<MultipleHitsQuery, MultipleHitsResult[]>
	{
		private readonly IRepository repository;

		public MultipleHitsHandler(IRepository repository)
		{
			this.repository = repository;
		}

		public async Task<MultipleHitsResult[]> Handle(
			MultipleHitsQuery query,
			CancellationToken cancellationToken)
		{
			var hits = await (
				from e in repository.Get<EventLog>()
				where
					(query.From == null || query.From <= e.Date) &&
					(query.To == null || query.To >= e.Date) &&
					e.Event == EventLog.EventType.HIT &&
					e.EnemyPlayerId != null &&
					e.Distance != null
				group e.Distance by new { e.EnemyPlayerId, e.Date, e.Weapon } into g
				select new MultipleHitsResult()
				{
					PlayerId = g.Key.EnemyPlayerId.Value,
					Date = g.Key.Date,
					Shots = g.Count(),
					Weapon = g.Key.Weapon,
					Distance = g.Average(x => x.Value),
				}).Where(x => x.Shots > 1).ToArrayAsync(cancellationToken);

			return hits;
		}
	}
}
