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
            var match = PlayerConnectedExp.Match(log);
            if (match.Success)
            {
				string time = match.Groups["time"].Value;
				string nickname = match.Groups["nickname"].Value;
				string guid = match.Groups["guid"].Value;

				var player = await GetPlayer(guid, cancellationToken);
				var nick = await GetNickname(player, nickname, cancellationToken);

				return GetSessionStart(player, time);
			}

			match = PlayerDisconnectedExp.Match(log);
			if (match.Success)
			{
				string time = match.Groups["time"].Value;
				string guid = match.Groups["guid"].Value;

				var player = await GetPlayer(guid, cancellationToken);
				
				return GetSessioEnd(player, time);
			}

			return null;
		}

        private async Task<Player> GetPlayer(
			string guid,
			CancellationToken cancellationToken)
        {
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
	}
}
