using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Selection.KillsByPlayer
{
	public class KillsByPlayerHandler : IRequestHandler<KillsByPlayerQuery, KillsByPlayerResult[]>
	{
		private readonly IRepository repository;

		public KillsByPlayerHandler(IRepository repository)
		{
			this.repository = repository;
		}

		public async Task<KillsByPlayerResult[]> Handle(
			KillsByPlayerQuery query,
			CancellationToken cancellationToken)
		{
			var playerKills = await (
				from e in repository.Get<EventLog>()
				where
					(query.From == null || query.From <= e.Date) &&
					(query.To == null || query.To >= e.Date) &&
					e.Event == EventLog.EventType.MURDER
				group e.Id by e.EnemyPlayerId into g
				select new KillsByPlayerResult()
				{
					PlayerId = g.Key.Value,
					Kills = g.Count(),
				}).ToArrayAsync(cancellationToken);

			return playerKills;
		}
	}
}
