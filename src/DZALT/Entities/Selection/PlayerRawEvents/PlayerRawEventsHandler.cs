using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Selection.PlayerRawEvents
{
	public record PlayerRawEventsHandler : IRequestHandler<PlayerRawEventsQuery, EventLog[]>
	{
		private readonly IRepository repository;

		public PlayerRawEventsHandler(IRepository repository)
		{
			this.repository = repository;
		}

		public async Task<EventLog[]> Handle(
			PlayerRawEventsQuery query,
			CancellationToken cancellationToken)
		{
			var playerId = await repository.PlayerIdByName(query.PlayerNickOrGuid, cancellationToken);
			var playerNames = await repository.PlayersNames(cancellationToken);

			var logs = await repository.Get<EventLog>()
				.Where(x => x.PlayerId == playerId)
				.OrderBy(x => x.Date)
				.ToArrayAsync(cancellationToken);
			
			return logs;
		}
	}
}
