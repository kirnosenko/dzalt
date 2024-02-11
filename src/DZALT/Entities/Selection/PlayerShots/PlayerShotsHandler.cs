using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Selection.PlayerShots
{
	public class PlayerShotsHandler : IRequestHandler<PlayerShotsQuery, PlayerShotsResult[]>
	{
		private readonly IRepository repository;

		public PlayerShotsHandler(IRepository repository)
		{
			this.repository = repository;
		}

		public async Task<PlayerShotsResult[]> Handle(
			PlayerShotsQuery query,
			CancellationToken cancellationToken)
		{
			var playerShots = await (
				from e in repository.Get<EventLog>()
				where
					(query.From == null || query.From <= e.Date) &&
					(query.To == null || query.To >= e.Date) &&
					e.Event == EventLog.EventType.HIT &&
					e.EnemyPlayerId != null &&
					e.Distance != null
				select new
				{
					e.Date,
					e.PlayerId,
					e.EnemyPlayerId,
					e.Weapon,
					e.BodyPart,
					e.Distance,
				}).ToArrayAsync(cancellationToken);

			return playerShots
				.Where(x =>
					query.Bodyparts == null ||
					query.Bodyparts.Length == 0 ||
					query.Bodyparts.Any(bp => x.BodyPart.StartsWith(bp)))
				.Select(x => new PlayerShotsResult()
				{
					Date = x.Date,
					AttackerId = x.EnemyPlayerId.Value,
					VictimId = x.PlayerId,
					Weapon = x.Weapon,
					Bodypart = x.BodyPart,
					Distance = x.Distance.Value,
				}).ToArray();
		}
	}
}
