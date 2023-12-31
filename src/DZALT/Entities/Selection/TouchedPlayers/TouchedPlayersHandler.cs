using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Selection.TouchedPlayers
{
	public class TouchedPlayersHandler : IRequestHandler<TouchedPlayersQuery, TouchedPlayersResult[]>
	{
		private readonly IRepository repository;

		public TouchedPlayersHandler(IRepository repository)
		{
			this.repository = repository;
		}

		public async Task<TouchedPlayersResult[]> Handle(
			TouchedPlayersQuery query,
			CancellationToken cancellationToken)
		{
			var playerNames = await repository.PlayersNames(cancellationToken);

			var touchedPlayers = await (
				from e in repository.Get<EventLog>()
				where
					(query.From == null || query.From <= e.Date) &&
					(query.To == null || query.To >= e.Date) &&
					e.Event == EventLog.EventType.HIT &&
					e.EnemyPlayerId != null &&
					e.Distance == null
				group e.PlayerId by e.EnemyPlayerId into g
				select new
				{
					Id = g.Key.Value,
					Touched = g.Distinct().Count(),
				}).ToArrayAsync(cancellationToken);

			return touchedPlayers
				.OrderByDescending(x => x.Touched)
				.Select(x => new TouchedPlayersResult()
				{
					Name = playerNames[x.Id],
					Touched = x.Touched,
				}).ToArray();
		}
	}
}
