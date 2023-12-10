using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DZALT.Entities.Selection.NamesByPlayer;

namespace DZALT.Entities.Selection.LongestShots
{
	public class LongestShotsHandler : IRequestHandler<LongestShotsQuery, LongestShotsResult[]>
	{
		private readonly IMediator mediator;
		private readonly IRepository repository;

		public LongestShotsHandler(
			IMediator mediator,
			IRepository repository)
		{
			this.mediator = mediator;
			this.repository = repository;
		}

		public async Task<LongestShotsResult[]> Handle(
			LongestShotsQuery query,
			CancellationToken cancellationToken)
		{
			var playerNicknames = await mediator.Send(
				NamesByPlayerQuery.Instance,
				cancellationToken);

			var playerShots = await (
				from e in repository.Get<EventLog>()
				where
					(query.From == null || query.From <= e.Date) &&
					(query.To == null || query.To >= e.Date) &&
					e.Event == EventLog.EventType.HIT &&
					e.EnemyPlayerId != null &&
					e.Distance != null
				orderby e.Distance descending
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
				.Select(x => new LongestShotsResult()
				{
					Date = x.Date,
					Attacker = playerNicknames[x.EnemyPlayerId.Value],
					Victim = playerNicknames[x.PlayerId],
					Weapon = x.Weapon,
					Bodypart = x.BodyPart,
					Distance = x.Distance.Value,
				}).ToArray();
		}
	}
}
