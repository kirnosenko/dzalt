using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
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
					var pt = time.SingleOrDefault(t => t.PlayerId == x.PlayerId);

					return new KillsPerHourByPlayerResult()
					{
						PlayerId = x.PlayerId,
						Kills = x.Kills,
						Time = pt != null ? pt.Time : TimeSpan.FromHours(0),
						KillsPerHour = pt != null ? (decimal)(x.Kills / pt.Time.TotalHours) : decimal.MaxValue,
					};
				}).ToArray();

			return killsPerHour;
		}
	}
}
