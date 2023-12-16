using System;
using MediatR;

namespace DZALT.Entities.Selection.KillsPerHourByPlayer
{
	public record KillsPerHourByPlayerQuery : IRequest<KillsPerHourByPlayerResult[]>
	{
		private KillsPerHourByPlayerQuery()
		{
		}

		public DateTime? From { get; set; }
		public DateTime? To { get; set; }

		public static KillsPerHourByPlayerQuery Create(
			DateTime? from = null,
			DateTime? to = null)
			=> new KillsPerHourByPlayerQuery()
			{
				From = from,
				To = to,
			};
	}
}
