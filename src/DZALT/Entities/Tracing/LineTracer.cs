using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Tracing
{
	public interface ILineTracer
	{
		Task<Log> Trace(string log, CancellationToken cancellationToken);
	}

	public class LineTracer : ILineTracer
	{
		private static readonly Regex SurvivorNicknameExp = new Regex(
			@"^Survivor( \(\d+\))?$");
		private static readonly Regex PlayerConnectedExp = new Regex(
			@"^(?<time>.*?) \| Player ""(?<nickname>.*?)"" is connected \(id=(?<guid>.*?)\)$");
		private static readonly Regex PlayerDisconnectedExp = new Regex(
			@"^(?<time>.*?) \| Player ""(?<nickname>.*?)""\(id=(?<guid>.*?)\) has been disconnected$");
		private static readonly Regex PlayerHitExp = new Regex(
			@"^(?<time>.*?) \| Player ""(?<nickname>.*?)"" \(id=(?<guid>.*?) pos=<(?<x>.*?), (?<y>.*?), (?<z>.*?)>\)\[HP: (?<health>.*?)\] hit by (?<enemy>.*?) into (?<bodypart>.*?) for (?<damage>.*?) damage \((?<hitter>.*?)\)( with (?<weapon>.*?) from (?<distance>.*?) meters)?$");
		private static readonly Regex PlayerExp = new Regex(
			@"^Player ""(?<nickname>.*?)"" \(id=(?<guid>.*?) pos=<(?<x>.*?), (?<y>.*?), (?<z>.*?)>\)$");

		private readonly IRepository repository;
        private readonly Dictionary<string, Player> players;
        private readonly Dictionary<string, Nickname> nicknames;
        
        public LineTracer(IRepository repository)
        {
            this.repository = repository;
            players = new Dictionary<string, Player>();
            nicknames = new Dictionary<string, Nickname>();
        }

        public async Task<Log> Trace(string log, CancellationToken cancellationToken)
        {
			Match match = null;

			match = PlayerHitExp.Match(log);
			if (match.Success)
			{
				return await GetEventLog(match, cancellationToken);
			}

			match = PlayerConnectedExp.Match(log);
            if (match.Success)
            {
				string time = match.Groups["time"].Value;
				string nickname = match.Groups["nickname"].Value;
				string guid = match.Groups["guid"].Value;

				var player = await GetPlayer(guid, cancellationToken);
				if (player == null)
				{
					return null;
				}
				var nick = await GetNickname(player, nickname, cancellationToken);

				return GetSessionStart(player, time);
			}

			match = PlayerDisconnectedExp.Match(log);
			if (match.Success)
			{
				string time = match.Groups["time"].Value;
				string guid = match.Groups["guid"].Value;

				var player = await GetPlayer(guid, cancellationToken);
				if (player == null)
				{
					return null;
				}

				return GetSessioEnd(player, time);
			}

			return null;
		}

        private async Task<Player> GetPlayer(
			string guid,
			CancellationToken cancellationToken)
        {
			if (guid == "Unknown")
			{
				return null;
			}

            if (!players.TryGetValue(guid, out var player))
            {
				player = await repository.GetUpdatable<Player>()
					.SingleOrDefaultAsync(x => x.Guid == guid, cancellationToken);

				if (player == null)
				{
					player = new Player()
					{
						Guid = guid,
					};
					repository.Add(player);
				}

				players.Add(guid, player);
			}

            return player;
        }

		private async Task<Nickname> GetNickname(
			Player player,
			string nickname,
			CancellationToken cancellationToken)
		{
			if (SurvivorNicknameExp.IsMatch(nickname))
			{
				return null;
			}

			if (!nicknames.TryGetValue(player.Guid + nickname, out var nick))
			{
				nick = await (
					from n in repository.GetUpdatable<Nickname>()
					join p in repository.GetUpdatable<Player>()
						on n.PlayerId equals p.Id
					where p.Guid == player.Guid && n.Name == nickname
					select n
				).SingleOrDefaultAsync(cancellationToken);

				if (nick == null)
				{
					nick = new Nickname()
					{
						Name = nickname,
						Player = player,
					};
					repository.Add(nick);
				}

				nicknames.Add(player.Guid + nickname, nick);
			}

			return nick;
		}

		private SessionLog GetSessionStart(Player player, string time)
		{
			var sessionLog = new SessionLog()
			{
				Player = player,
				Date = new DateTime(0) + TimeOnly.Parse(time).ToTimeSpan(),
				Type = SessionLog.Reason.CONNECTED,
			};

			repository.Add(sessionLog);

			return sessionLog;
		}

		private SessionLog GetSessioEnd(Player player, string time)
		{
			var sessionLog = new SessionLog()
			{
				Player = player,
				Date = new DateTime(0) + TimeOnly.Parse(time).ToTimeSpan(),
				Type = SessionLog.Reason.DISCONNECTED,
			};

			repository.Add(sessionLog);

			return sessionLog;
		}

		private async Task<EventLog> GetEventLog(Match match, CancellationToken cancellationToken)
		{
			string time = match.Groups["time"].Value;
			string guid = match.Groups["guid"].Value;
			string x = match.Groups["x"].Value;
			string y = match.Groups["y"].Value;
			string z = match.Groups["z"].Value;
			string damage = match.Groups["damage"].Value;
			string health = match.Groups["health"].Value;
			string enemy = match.Groups["enemy"].Value;
			string bodypart = match.Groups["bodypart"].Value;
			string hitter = match.Groups["hitter"].Value;

			var player = await GetPlayer(guid, cancellationToken);
			if (player == null)
			{
				return null;
			}

			var eventLog = new EventLog()
			{
				Player = player,
				Date = new DateTime(0) + TimeOnly.Parse(time).ToTimeSpan(),
				X = decimal.TryParse(x, out var xValue) ? xValue : 0,
				Y = decimal.TryParse(y, out var yValue) ? yValue : 0,
				Z = decimal.TryParse(z, out var zValue) ? zValue : 0,
				Event = EventLog.EventType.HIT,
				Damage = decimal.TryParse(damage, out var damageValue) ? damageValue : null,
				Health = decimal.TryParse(health, out var healthValue) ? healthValue : null,
				BodyPart = bodypart,
				Hitter = hitter,
			};

			var enemyMatch = PlayerExp.Match(enemy);
			if (enemyMatch.Success)
			{
				string enemyGuid = enemyMatch.Groups["guid"].Value;
				string enemyX = enemyMatch.Groups["x"].Value;
				string enemyY = enemyMatch.Groups["y"].Value;
				string enemyZ = enemyMatch.Groups["z"].Value;
				string weapon = match.Groups["weapon"].Value;
				string distance = match.Groups["distance"].Value;

				var enemyPlayer = await GetPlayer(enemyGuid, cancellationToken);
				eventLog.EnemyPlayer = enemyPlayer;
				eventLog.EnemyPlayerX = decimal.TryParse(enemyX, out var enemyXValue) ? enemyXValue : 0;
				eventLog.EnemyPlayerY = decimal.TryParse(enemyY, out var enemyYValue) ? enemyYValue : 0;
				eventLog.EnemyPlayerZ = decimal.TryParse(enemyZ, out var enemyZValue) ? enemyZValue : 0;
				eventLog.Weapon = weapon;
				eventLog.Distance = decimal.TryParse(distance, out var distanceValue) ? distanceValue : null;
			}
			else
			{
				eventLog.Enemy = enemy;
			}
			
			repository.Add(eventLog);

			return eventLog;
		}
	}
}
