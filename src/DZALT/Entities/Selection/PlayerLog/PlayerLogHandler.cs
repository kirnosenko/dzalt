﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DZALT.Entities.Selection.NamesByPlayer;

namespace DZALT.Entities.Selection.PlayerLog
{
	public record PlayerLogHandler : IRequestHandler<PlayerLogQuery, string[]>
	{
		private readonly IMediator mediator;
		private readonly IRepository repository;

		public PlayerLogHandler(
			IMediator mediator,
			IRepository repository)
		{
			this.mediator = mediator;
			this.repository = repository;
		}

		public async Task<string[]> Handle(
			PlayerLogQuery query,
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

			var sessions = await repository.Get<SessionLog>()
				.Where(x => x.PlayerId == playerId)
				.ToArrayAsync(cancellationToken);
			var events = await repository.Get<EventLog>()
				.Where(x =>
					x.EnemyPlayer != null && x.Event == EventLog.EventType.MURDER &&
					(x.PlayerId == playerId || x.EnemyPlayerId == playerId))
				.ToArrayAsync(cancellationToken);

			var sessionsLogs = sessions.Select(s =>
			{
				var log = s.Type == SessionLog.SessionType.CONNECTED
					? $"{playerName} connected."
					: $"{playerName} disconnected.";
				return (s.Date, log);
			}).ToArray();
			var eventsLogs = events.Select(e =>
			{
				string victum = e.PlayerId == playerId ? playerName : playerNicknames[e.PlayerId];
				string attacker = e.EnemyPlayerId == playerId ? playerName : playerNicknames[e.EnemyPlayerId.Value];
				var distance = e.Distance == null ? "." : $" from {e.Distance} meters.";
				var log = $"{victum} killed by {attacker} with {e.Weapon}{distance}";

				return (e.Date, log);
			}).ToArray();

			return sessionsLogs.Concat(eventsLogs)
				.OrderBy(x => x.Date)
				.Select(x => $"{x.Date}: {x.log}")
				.ToArray();
		}
	}
}
