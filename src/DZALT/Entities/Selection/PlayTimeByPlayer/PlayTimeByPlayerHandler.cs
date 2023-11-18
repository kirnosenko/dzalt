using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Selection.PlayTimeByPlayer
{
	public class PlayTimeByPlayerHandler : IRequestHandler<PlayTimeByPlayerQuery, PlayTimeByPlayerResult[]>
	{
		private readonly IRepository repository;

		public PlayTimeByPlayerHandler(IRepository repository)
		{
			this.repository = repository;
		}

		public async Task<PlayTimeByPlayerResult[]> Handle(
			PlayTimeByPlayerQuery query,
			CancellationToken cancellationToken)
		{
			var playerNicknames = await (
				from p in repository.Get<Player>()
				let name = repository.Get<Nickname>()
					.Where(x => x.PlayerId == p.Id)
					.Select(x => x.Name)
					.FirstOrDefault()
				select new
				{
					Id = p.Id,
					Guid = p.Guid,
					Name = name,
				}).ToDictionaryAsync(x => x.Id, x => x.Name ?? x.Guid, cancellationToken);

			var playerSessions = await (
				from sc in repository.Get<SessionLog>()
					.Where(x => x.Type == SessionLog.Reason.CONNECTED)
				from sd in repository.Get<SessionLog>()
					.Where(x => x.PlayerId == sc.PlayerId && x.Type == SessionLog.Reason.DISCONNECTED && x.Date >= sc.Date)
					.OrderBy(x => x.Date)
					.Take(1)
				select new
				{
					Id = sc.PlayerId,
					From = sc.Date,
					To = sd.Date,
				}).ToArrayAsync(cancellationToken);

			var playerTimes =
				from ps in playerSessions
				group ps.To.Ticks - ps.From.Ticks by ps.Id into g
				select new PlayTimeByPlayerResult()
				{
					Name = playerNicknames[g.Key],
					Time = TimeSpan.FromTicks(g.Sum())
				};

			return playerTimes.OrderByDescending(x => x.Time).ToArray();
		}
	}
}
