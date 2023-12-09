using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DZALT.Entities.Selection.KillsByPlayer;
using DZALT.Entities.Selection.PlayTimeByPlayer;

namespace DZALT.Entities.Selection.KillsPerHourByPlayer
{
	public class KillsPerHourByPlayerHandler : IRequestHandler<KillsPerHourByPlayerQuery, KillsPerHourByPlayerResult[]>
	{
		private readonly IMediator mediator;
		
		public KillsPerHourByPlayerHandler(
			IMediator mediator)
		{
			this.mediator = mediator;
		}

		public async Task<KillsPerHourByPlayerResult[]> Handle(
			KillsPerHourByPlayerQuery query,
			CancellationToken cancellationToken)
		{
			var kills = await mediator.Send(
				KillsByPlayerQuery.Create(query.From, query.To),
				cancellationToken);

			var time = await mediator.Send(
				PlayTimeByPlayerQuery.Create(query.From, query.To),
				cancellationToken);

			var killsPerHour = kills
				.Select(x =>
				{
					var tt = time.Single(t => t.Name == x.Name);
					return new KillsPerHourByPlayerResult()
					{
						Name = x.Name,
						Kills = x.Kills,
						Time = tt.Time,
						KillsPerHour = (decimal)(x.Kills / tt.Time.TotalHours)
					};
				}).OrderByDescending(x => x.KillsPerHour).ToArray();

			return killsPerHour;
		}
	}
}
