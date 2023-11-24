using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Selection.PlayTimeIntersection
{
	public class PlayTimeIntersectionHandler : IRequestHandler<PlayTimeIntersectionQuery, decimal>
	{
		private readonly IRepository repository;

		public PlayTimeIntersectionHandler(IRepository repository)
		{
			this.repository = repository;
		}

		public async Task<decimal> Handle(
			PlayTimeIntersectionQuery query,
			CancellationToken cancellationToken)
		{
			var player1Id = await (
				from p in repository.Get<Player>()
				join n in repository.Get<Nickname>()
					on p.Id equals n.PlayerId into leftjoin
				from x in leftjoin.DefaultIfEmpty()
				where p.Guid == query.P1NickOrGuid || x.Name == query.P1NickOrGuid
				select p.Id).FirstOrDefaultAsync(cancellationToken);
			var player2Id = await (
				from p in repository.Get<Player>()
				join n in repository.Get<Nickname>()
					on p.Id equals n.PlayerId into leftjoin
				from x in leftjoin.DefaultIfEmpty()
				where p.Guid == query.P2NickOrGuid || x.Name == query.P2NickOrGuid
				select p.Id).FirstOrDefaultAsync(cancellationToken);

			var playerSessions = await (
				from sc in repository.Get<SessionLog>()
					.Where(x => x.Type == SessionLog.SessionType.CONNECTED)
				from sd in repository.Get<SessionLog>()
					.Where(x => x.PlayerId == sc.PlayerId && x.Type == SessionLog.SessionType.DISCONNECTED && x.Date >= sc.Date)
					.OrderBy(x => x.Date)
					.Take(1)
				where
					(query.From == null || query.From < sd.Date) &&
					(query.To == null || query.To > sc.Date) &&
					(sc.PlayerId == player1Id || sc.PlayerId == player2Id)
				select new
				{
					Id = sc.PlayerId,
					From = sc.Date,
					To = sd.Date,
				}).ToArrayAsync(cancellationToken);

			var playTimeSum = playerSessions
				.Sum(x => (query.To != null && query.To < x.To ? query.To.Value : x.To).Ticks - (query.From != null && query.From > x.From ? query.From.Value : x.From).Ticks);

			var sessionMarks = playerSessions
				.Select(x => (1, x.From))
				.ToArray()
				.Concat(playerSessions
					.Select(x => (0, x.To))
					.ToArray())
				.OrderBy(x => x.Item2)
				.Select(x => x)
				.ToArray();

			long playTimeIntersection = 0;
			int counter = 0;
			DateTime playTimeIntersectionFrom = DateTime.MinValue;
			foreach (var mark in sessionMarks)
			{
				var newCounter = counter + (mark.Item1 > 0 ? 1 : -1);
				if (newCounter == 2 && counter == 1)
				{
					playTimeIntersectionFrom = mark.Item2;
				}
				else if (newCounter == 1 && counter == 2)
				{
					playTimeIntersection += (mark.Item2 - playTimeIntersectionFrom).Ticks;
				}
				counter = newCounter;
			}

			return playTimeIntersection / (playTimeSum / 2.0M);
		}
	}
}
