using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DZALT.Entities.Selection.NamesByPlayer;

namespace DZALT.Entities.Selection.PlayerRawEvents
{
	public record PlayerRawEventsHandler : IRequestHandler<PlayerRawEventsQuery, EventLog[]>
	{
		private readonly IMediator mediator;
		private readonly IRepository repository;

		public PlayerRawEventsHandler(
			IMediator mediator,
			IRepository repository)
		{
			this.mediator = mediator;
			this.repository = repository;
		}

		public async Task<EventLog[]> Handle(
			PlayerRawEventsQuery query,
			CancellationToken cancellationToken)
		{
			var playerName = query.PlayerNickOrGuid;
			var playerId = await (
				from p in repository.Get<Player>()
				join n in repository.Get<Nickname>()
					on p.Id equals n.PlayerId into leftjoin
				from x in leftjoin.DefaultIfEmpty()
				where p.Guid == playerName || x.Name == playerName
				select p.Id).FirstOrDefaultAsync(cancellationToken);

			var playerNicknames = await mediator.Send(
				NamesByPlayerQuery.Instance,
				cancellationToken);

			var logs = await repository.Get<EventLog>()
				.Where(x => x.PlayerId == playerId)
				.OrderBy(x => x.Date)
				.ToArrayAsync(cancellationToken);
			
			return logs;
		}
	}
}
