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

		public static KillsPerHourByPlayerQuery Create()
			=> Create(null, null);

		public static KillsPerHourByPlayerQuery Create(
			DateTime? from,
			DateTime? to)
			=> new KillsPerHourByPlayerQuery()
			{
				From = from,
				To = to,
			};
	}
}
