﻿using System;
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
		private static readonly Regex PlayerUnconsciousExp = new Regex(
			@"^(?<time>.*?) \| Player ""(?<nickname>.*?)"" (\(DEAD\) )?\(id=(?<guid>.*?) pos=<(?<x>.*?), (?<y>.*?), (?<z>.*?)>\) is unconscious$");
		private static readonly Regex PlayerConsciousExp = new Regex(
			@"^(?<time>.*?) \| Player ""(?<nickname>.*?)"" \(id=(?<guid>.*?) pos=<(?<x>.*?), (?<y>.*?), (?<z>.*?)>\) regained consciousness$");
		private static readonly Regex PlayerDeadExp = new Regex(
			@"^(?<time>.*?) \| Player ""(?<nickname>.*?)"" (\(DEAD\) )?\(id=(?<guid>.*?) pos=<(?<x>.*?), (?<y>.*?), (?<z>.*?)>\) died. Stats> (?<stats>.*?)$");
		private static readonly Regex PlayerSuicideExp = new Regex(
			@"^(?<time>.*?) \| Player '(?<nickname>.*?)' \(id=(?<guid>.*?)\) committed suicide.$");
		private static readonly Regex PlayerSuicideExp2 = new Regex(
			@"^(?<time>.*?) \| Player ""(?<nickname>.*?)"" \(id=(?<guid>.*?) pos=<(?<x>.*?), (?<y>.*?), (?<z>.*?)>\) committed suicide$");
		private static readonly Regex PlayerHitExp = new Regex(
			@"^(?<time>.*?) \| Player ""(?<nickname>.*?)"" (\(DEAD\) )?\(id=(?<guid>.*?) pos=<(?<x>.*?), (?<y>.*?), (?<z>.*?)>\)(\[HP: (?<health>.*?)\])? (?<action>(hit|killed)) by (?<enemy>.*?)( into (?<bodypart>.*?) for (?<damage>.*?) damage \((?<hitter>.*?)\))?( with (?<weapon>.*?)( from (?<distance>.*?) meters )?)?$");
		private static readonly Regex PlayerBledOutExp = new Regex(
			@"^(?<time>.*?) \| Player ""(?<nickname>.*?)"" (\(DEAD\) )?\(id=(?<guid>.*?) pos=<(?<x>.*?), (?<y>.*?), (?<z>.*?)>\) bled out$");
		private static readonly Regex PlayerBuiltExp = new Regex(
			@"^(?<time>.*?) \| Player ""(?<nickname>.*?)"" \(id=(?<guid>.*?) pos=<(?<x>.*?), (?<y>.*?), (?<z>.*?)>\) built .*?$");
		private static readonly Regex PlayerExp = new Regex(
			@"^Player ""(?<nickname>.*?)"" (\(DEAD\) )?\(id=(?<guid>.*?) pos=<(?<x>.*?), (?<y>.*?), (?<z>.*?)>\)$");

		private readonly IRepository repository;
		private readonly Dictionary<string, Player> players;
		private readonly Dictionary<string, Nickname> nicknames;
		private readonly Queue<Log> logs;

		public LineTracer(IRepository repository)
		{
			this.repository = repository;
			players = new Dictionary<string, Player>();
			nicknames = new Dictionary<string, Nickname>();
			logs = new Queue<Log>();
		}

		public async Task<Log> Trace(string line, CancellationToken cancellationToken)
		{
			Log log = line.Contains("| Player") ? await TraceInner(line, cancellationToken) : null;

			if (log != null)
			{
				logs.Enqueue(log);
			}

			return log;
		}

		private async Task<Log> TraceInner(string line, CancellationToken cancellationToken)
		{
			Match match = null;

			match = PlayerHitExp.Match(line);
			if (match.Success)
			{
				return await GetEventLogForHit(match, cancellationToken);
			}

			match = PlayerDeadExp.Match(line);
			if (match.Success)
			{
				return await GetEventLogForDead(match, cancellationToken);
			}

			match = PlayerSuicideExp.Match(line);
			if (match.Success)
			{
				return await GetEventLogForSuicide(match, cancellationToken);
			}

			match = PlayerSuicideExp2.Match(line);
			if (match.Success)
			{
				return await GetEventLogForSuicide(match, cancellationToken);
			}

			match = PlayerConnectedExp.Match(line);
			if (match.Success)
			{
				return await GetSessionStart(match, cancellationToken);
			}

			match = PlayerDisconnectedExp.Match(line);
			if (match.Success)
			{
				return await GetSessionEnd(match, cancellationToken);
			}

			match = PlayerUnconsciousExp.Match(line);
			if (match.Success)
			{
				return await GetEventForConsciousness(match, EventLog.EventType.UNCONSCIOUS, cancellationToken);
			}

			match = PlayerConsciousExp.Match(line);
			if (match.Success)
			{
				return await GetEventForConsciousness(match, EventLog.EventType.CONSCIOUS, cancellationToken);
			}

			if (PlayerBledOutExp.IsMatch(line)) return null;
			if (PlayerBuiltExp.IsMatch(line)) return null;

			throw new ApplicationException($"Could not trace log: {line}");
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

		private async Task<Log> GetSessionStart(Match match, CancellationToken cancellationToken)
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

			return GetSession(player, time, SessionLog.SessionType.CONNECTED);
		}

		private async Task<Log> GetSessionEnd(Match match, CancellationToken cancellationToken)
		{
			string time = match.Groups["time"].Value;
			string guid = match.Groups["guid"].Value;

			var player = await GetPlayer(guid, cancellationToken);
			if (player == null)
			{
				return null;
			}

			return GetSession(player, time, SessionLog.SessionType.DISCONNECTED);
		}

		private Log GetSession(Player player, string time, SessionLog.SessionType type)
		{
			var sessionLog = new SessionLog()
			{
				Player = player,
				Date = new DateTime(0) + TimeOnly.Parse(time).ToTimeSpan(),
				Type = type,
			};

			repository.Add(sessionLog);

			return sessionLog;
		}

		private async Task<Log> GetEventForConsciousness(Match match, EventLog.EventType @event, CancellationToken cancellationToken)
		{
			string time = match.Groups["time"].Value;
			string guid = match.Groups["guid"].Value;
			string x = match.Groups["x"].Value;
			string y = match.Groups["y"].Value;
			string z = match.Groups["z"].Value;

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
				Event = @event,
			};

			repository.Add(eventLog);

			return eventLog;
		}

		private async Task<Log> GetEventLogForDead(Match match, CancellationToken cancellationToken)
		{
			string time = match.Groups["time"].Value;
			string guid = match.Groups["guid"].Value;
			string x = match.Groups["x"].Value;
			string y = match.Groups["y"].Value;
			string z = match.Groups["z"].Value;
			
			var player = await GetPlayer(guid, cancellationToken);
			if (player == null)
			{
				return null;
			}

			var logTime = TimeOnly.Parse(time);
			var eventLog = FindLastEventLog(x =>
				x.Player == player &&
				x.Event == EventLog.EventType.SUICIDE);

			if (eventLog != null && logTime == TimeOnly.FromDateTime(eventLog.Date))
			{
				return null;
			}

			eventLog = new EventLog()
			{
				Player = player,
				Date = new DateTime(0) + TimeOnly.Parse(time).ToTimeSpan(),
				X = decimal.TryParse(x, out var xValue) ? xValue : 0,
				Y = decimal.TryParse(y, out var yValue) ? yValue : 0,
				Z = decimal.TryParse(z, out var zValue) ? zValue : 0,
				Event = EventLog.EventType.ACCIDENT,
			};

			repository.Add(eventLog);
			return eventLog;
		}

		private async Task<Log> GetEventLogForSuicide(Match match, CancellationToken cancellationToken)
		{
			string time = match.Groups["time"].Value;
			string guid = match.Groups["guid"].Value;
			string x = match.Groups["x"].Value;
			string y = match.Groups["y"].Value;
			string z = match.Groups["z"].Value;

			var player = await GetPlayer(guid, cancellationToken);
			if (player == null)
			{
				return null;
			}

			var logTime = TimeOnly.Parse(time);
			var eventLog = FindLastEventLog(x =>
				x.Player == player &&
				x.Event == EventLog.EventType.ACCIDENT);

			if (eventLog != null && logTime == TimeOnly.FromDateTime(eventLog.Date))
			{
				eventLog.Event = EventLog.EventType.SUICIDE;
				return null;
			}

			eventLog = new EventLog()
			{
				Player = player,
				Date = new DateTime(0) + logTime.ToTimeSpan(),
				X = decimal.TryParse(x, out var xValue) ? xValue : 0,
				Y = decimal.TryParse(y, out var yValue) ? yValue : 0,
				Z = decimal.TryParse(z, out var zValue) ? zValue : 0,
				Event = EventLog.EventType.SUICIDE,
			};

			repository.Add(eventLog);
			return eventLog;
		}

		private async Task<Log> GetEventLogForHit(Match match, CancellationToken cancellationToken)
		{
			string time = match.Groups["time"].Value;
			string guid = match.Groups["guid"].Value;
			string x = match.Groups["x"].Value;
			string y = match.Groups["y"].Value;
			string z = match.Groups["z"].Value;
			string damage = match.Groups["damage"].Value;
			string health = match.Groups["health"].Value;
			string action = match.Groups["action"].Value;
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
				Damage = decimal.TryParse(damage, out var damageValue) ? damageValue : null,
				Health = decimal.TryParse(health, out var healthValue) ? healthValue : null,
				BodyPart = bodypart != string.Empty ? bodypart : null,
				Hitter = hitter != string.Empty ? hitter : null,
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
				eventLog.Event = action == "hit" ? EventLog.EventType.HIT : EventLog.EventType.MURDER;
				eventLog.Weapon = weapon != string.Empty ? weapon : null;
				eventLog.Distance = decimal.TryParse(distance, out var distanceValue) ? distanceValue : null;
			}
			else
			{
				eventLog.Event = action == "hit" ? EventLog.EventType.HIT : EventLog.EventType.ACCIDENT;
				eventLog.Enemy = enemy;
			}
			
			repository.Add(eventLog);

			return eventLog;
		}

		private EventLog FindLastEventLog(Func<EventLog, bool> filter)
		{
			var log = logs
				.Where(x => x is EventLog)
				.Where(x => filter(x as EventLog))
				.OrderByDescending(x => x.Date)
				.FirstOrDefault();

			return log as EventLog;
		}
	}
}
