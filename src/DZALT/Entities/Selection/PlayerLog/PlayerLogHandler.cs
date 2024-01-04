using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Selection.PlayerLog
{
	public record PlayerLogHandler : IRequestHandler<PlayerLogQuery, PlayerLog[]>
	{
		private readonly IRepository repository;

		public PlayerLogHandler(IRepository repository)
		{
			this.repository = repository;
		}

		public async Task<PlayerLog[]> Handle(
			PlayerLogQuery query,
			CancellationToken cancellationToken)
		{
			var playerName = query.PlayerNickOrGuid;
			var playerId = await repository.PlayerIdByName(playerName, cancellationToken);
			var playerNames = await repository.PlayersNames(cancellationToken);

			var sessions = !query.IncludeSessions ? null :
				await repository.Get<SessionLog>()
					.Where(x => x.PlayerId == playerId)
					.ToArrayAsync(cancellationToken);
			var sessionsLogs = sessions
				?.Select<SessionLog, PlayerLog>(s => s.Type == SessionLog.SessionType.CONNECTED
					? PlayerConnectedLog.Create(s.Date, playerName)
					: PlayerDisconnectedLog.Create(s.Date, playerName))
				?.ToArray() ?? Array.Empty<PlayerLog>();

			var kills = !query.IncludeKills ? null : await (
				from murder in repository.Get<EventLog>()
					.Where(x =>
						x.EnemyPlayerId != null &&
						x.Event == EventLog.EventType.MURDER &&
						(x.PlayerId == playerId || x.EnemyPlayerId == playerId))
				from hit in repository.Get<EventLog>()
					.Where(x => 
						x.PlayerId == murder.PlayerId &&
						x.EnemyPlayerId == murder.EnemyPlayerId &&
						x.Event == EventLog.EventType.HIT &&
						x.Date <= murder.Date)
					.OrderByDescending(x => x.Date)
					.Take(1)
				select new
				{
					murder.Date,
					murder.PlayerId,
					murder.X,
					murder.Y,
					murder.Z,
					murder.EnemyPlayerId,
					murder.EnemyPlayerX,
					murder.EnemyPlayerY,
					murder.EnemyPlayerZ,
					murder.Distance,
					murder.Weapon,
					hit.BodyPart,
				}).ToArrayAsync(cancellationToken);
			var killsLogs = kills
				?.Select(e => PlayerKilledLog.Create(
					e.Date,
					e.PlayerId == playerId ? playerName : playerNames[e.PlayerId],
					(int)e.X,
					(int)e.Y,
					(int)e.Z,
					e.EnemyPlayerId == playerId ? playerName : playerNames[e.EnemyPlayerId.Value],
					(int)e.EnemyPlayerX,
					(int)e.EnemyPlayerY,
					(int)e.EnemyPlayerZ,
					e.Weapon,
					e.Distance.HasValue ? (int)e.Distance.Value : null,
					e.BodyPart))
				?.ToArray() ?? Array.Empty<PlayerLog>();

			var suicides = !query.IncludeSuicides ? null :
				await repository.Get<EventLog>()
					.Where(x => x.PlayerId == playerId && x.Event == EventLog.EventType.SUICIDE)
					.ToArrayAsync(cancellationToken);
			var suicidesLogs = suicides
				?.Select<EventLog, PlayerLog>(s => 
					PlayerSuicideLog.Create(
						s.Date,
						playerName,
						(int)s.X,
						(int)s.Y,
						(int)s.Z))
				?.ToArray() ?? Array.Empty<PlayerLog>();

			var accidents = !query.IncludeAccidents ? null :
				await repository.Get<EventLog>()
					.Where(x => x.PlayerId == playerId && x.Event == EventLog.EventType.ACCIDENT)
					.ToArrayAsync(cancellationToken);
			var accidentsLogs = accidents
				?.Select<EventLog, PlayerLog>(s =>
					PlayerAccidentLog.Create(
						s.Date,
						playerName,
						(int)s.X,
						(int)s.Y,
						(int)s.Z,
						s.Enemy))
				?.ToArray() ?? Array.Empty<PlayerLog>();

			return sessionsLogs
				.Concat(killsLogs)
				.Concat(suicidesLogs)
				.Concat(accidentsLogs)
				.OrderBy(x => x.Date)
				.ToArray();
		}
	}
}
