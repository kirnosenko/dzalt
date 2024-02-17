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
			var playerName = string.IsNullOrEmpty(query.PlayerNickOrGuid) ? null : query.PlayerNickOrGuid;
			var playerId = string.IsNullOrEmpty(playerName)
				? (int?)null
				: await repository.PlayerIdByName(playerName, cancellationToken);
			var playerNames = await repository.PlayersNames(cancellationToken);

			var sessions = !query.IncludeSessions ? null :
				await repository.Get<SessionLog>()
					.Where(l =>
						(query.From == null || query.From <= l.Date) &&
						(query.To == null || query.To >= l.Date) &&
						(playerId == null || playerId == l.PlayerId))
					.ToArrayAsync(cancellationToken);
			var sessionsLogs = sessions
				?.Select<SessionLog, PlayerLog>(s => s.Type == SessionLog.SessionType.CONNECTED
					? PlayerConnectLog.Create(s.Date, playerName ?? playerNames[s.PlayerId])
					: PlayerDisconnectLog.Create(s.Date, playerName ?? playerNames[s.PlayerId]))
				?.ToArray() ?? Array.Empty<PlayerLog>();

			var hits = !query.IncludeHits ? null : await (
				from hit in repository.Get<EventLog>()
				where
					(query.From == null || query.From <= hit.Date) &&
					(query.To == null || query.To >= hit.Date) &&
					hit.EnemyPlayerId != null &&
					hit.Event == EventLog.EventType.HIT &&
					(playerId == null || hit.PlayerId == playerId || hit.EnemyPlayerId == playerId)
				select new
				{
					hit.Date,
					hit.PlayerId,
					hit.X,
					hit.Y,
					hit.Z,
					hit.EnemyPlayerId,
					hit.EnemyPlayerX,
					hit.EnemyPlayerY,
					hit.EnemyPlayerZ,
					hit.Distance,
					hit.Weapon,
					hit.BodyPart,
				}).ToArrayAsync(cancellationToken);
			var hitsLogs = hits
				?.Select(e => PlayerHitLog.Create(
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

			var kills = !query.IncludeKills ? null : await (
				from kill in repository.Get<EventLog>()
					.Where(l =>
						(query.From == null || query.From <= l.Date) &&
						(query.To == null || query.To >= l.Date) &&
						l.EnemyPlayerId != null &&
						l.Event == EventLog.EventType.MURDER &&
						(playerId == null || l.PlayerId == playerId || l.EnemyPlayerId == playerId))
				from hit in repository.Get<EventLog>()
					.Where(l => 
						l.PlayerId == kill.PlayerId &&
						l.EnemyPlayerId == kill.EnemyPlayerId &&
						l.Event == EventLog.EventType.HIT &&
						l.Date <= kill.Date)
					.OrderByDescending(x => x.Date)
					.Take(1)
				select new
				{
					kill.Date,
					kill.PlayerId,
					kill.X,
					kill.Y,
					kill.Z,
					kill.EnemyPlayerId,
					kill.EnemyPlayerX,
					kill.EnemyPlayerY,
					kill.EnemyPlayerZ,
					kill.Distance,
					kill.Weapon,
					hit.BodyPart,
				}).ToArrayAsync(cancellationToken);
			var killsLogs = kills
				?.Select(e => PlayerKillLog.Create(
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
					.Where(l =>
						(query.From == null || query.From <= l.Date) &&
						(query.To == null || query.To >= l.Date) &&
						(playerId == null || l.PlayerId == playerId) &&
						l.Event == EventLog.EventType.SUICIDE)
					.ToArrayAsync(cancellationToken);
			var suicidesLogs = suicides
				?.Select<EventLog, PlayerLog>(s => 
					PlayerSuicideLog.Create(
						s.Date,
						playerName ?? playerNames[s.PlayerId],
						(int)s.X,
						(int)s.Y,
						(int)s.Z))
				?.ToArray() ?? Array.Empty<PlayerLog>();

			var accidents = !query.IncludeAccidents ? null :
				await repository.Get<EventLog>()
					.Where(l =>
						(query.From == null || query.From <= l.Date) &&
						(query.To == null || query.To >= l.Date) &&
						(playerId == null || l.PlayerId == playerId) &&
						l.Event == EventLog.EventType.ACCIDENT)
					.ToArrayAsync(cancellationToken);
			var accidentsLogs = accidents
				?.Select<EventLog, PlayerLog>(s =>
					PlayerAccidentLog.Create(
						s.Date,
						playerName ?? playerNames[s.PlayerId],
						(int)s.X,
						(int)s.Y,
						(int)s.Z,
						s.Enemy))
				?.ToArray() ?? Array.Empty<PlayerLog>();

			return sessionsLogs
				.Concat(hitsLogs)
				.Concat(killsLogs)
				.Concat(suicidesLogs)
				.Concat(accidentsLogs)
				.OrderBy(x => x.Date)
				.ToArray();
		}
	}
}
