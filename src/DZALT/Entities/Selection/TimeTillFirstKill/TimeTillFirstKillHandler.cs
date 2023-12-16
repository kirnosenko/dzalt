using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DZALT.Entities.Selection.NamesByPlayer;

namespace DZALT.Entities.Selection.TimeTillFirstKill
{
	public class TimeTillFirstKillHandler : IRequestHandler<TimeTillFirstKillQuery, TimeTillFirstKillResult[]>
	{
		private readonly IMediator mediator;
		private readonly IRepository repository;

		public TimeTillFirstKillHandler(
			IMediator mediator,
			IRepository repository)
		{
			this.mediator = mediator;
			this.repository = repository;
		}

		public async Task<TimeTillFirstKillResult[]> Handle(
			TimeTillFirstKillQuery query,
			CancellationToken cancellationToken)
		{
			var playerNicknames = await mediator.Send(
				NamesByPlayerQuery.Instance,
				cancellationToken);

			var playerKills = await (
				from kill in repository.Get<EventLog>()
					.Where(x =>
						x.Event == EventLog.EventType.MURDER &&
						x.Distance != null)
				from death in repository.Get<EventLog>()
					.Where(x =>
						x.PlayerId == kill.EnemyPlayerId &&
						x.Date < kill.Date &&
						(x.Event == EventLog.EventType.MURDER ||
						x.Event == EventLog.EventType.SUICIDE ||
						x.Event == EventLog.EventType.ACCIDENT))
					.OrderByDescending(x => x.Date)
					.Take(1)
				where
					(query.From == null || query.From <= death.Date) &&
					(query.To == null || query.To >= kill.Date)
				select new
				{
					Id = kill.EnemyPlayerId.Value,
					KillDate = kill.Date,
					DeathDate = death.Date,
					Time = kill.Date - death.Date
				}).OrderBy(x => x.Time).ToArrayAsync(cancellationToken);


			return playerKills
				.Select(x => new TimeTillFirstKillResult()
				{
					Name = playerNicknames[x.Id],
					DeathDate = x.DeathDate,
					KillDate = x.KillDate,
					Time = x.Time,
				}).ToArray();
		}
	}
}
