using System;
using MediatR;

namespace DZALT.Entities.Selection.TimeTillFirstKill
{
	public record TimeTillFirstKillQuery : IRequest<TimeTillFirstKillResult[]>
	{
		private TimeTillFirstKillQuery()
		{
		}

		public DateTime? From { get; set; }
		public DateTime? To { get; set; }

		public static TimeTillFirstKillQuery Create(
			DateTime? from = null,
			DateTime? to = null)
			=> new TimeTillFirstKillQuery()
			{
				From = from,
				To = to,
			};
	}
}
