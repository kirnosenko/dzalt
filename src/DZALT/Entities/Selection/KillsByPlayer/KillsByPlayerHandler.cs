using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DZALT.Entities.Selection.NamesByPlayer;

namespace DZALT.Entities.Selection.KillsByPlayer
{
	public class KillsByPlayerHandler : IRequestHandler<KillsByPlayerQuery, KillsByPlayerResult[]>
	{
		private readonly IMediator mediator;
		private readonly IRepository repository;

		public KillsByPlayerHandler(
			IMediator mediator,
			IRepository repository)
		{
			this.mediator = mediator;
			this.repository = repository;
		}

		public async Task<KillsByPlayerResult[]> Handle(
			KillsByPlayerQuery query,
			CancellationToken cancellationToken)
		{
			var playerNicknames = await mediator.Send(
				NamesByPlayerQuery.Instance,
				cancellationToken);
			
			var playerKills = await (
				from p in repository.Get<Player>()
				join e in repository.Get<EventLog>() on p.Id equals e.EnemyPlayerId
				where
					(query.From == null || query.From <= e.Date) &&
					(query.To == null || query.To >= e.Date) &&
					e.Event == EventLog.EventType.MURDER
				group e.Id by p.Id into g
				select new
				{
					Id = g.Key,
					Kills = g.Count(),
				}).ToArrayAsync(cancellationToken);

			return playerKills
				.OrderByDescending(x => x.Kills)
				.Select(x => new KillsByPlayerResult()
				{
					Name = playerNicknames[x.Id],
					Kills = x.Kills,
				}).ToArray();
		}
	}
}
