using System;
using MediatR;

namespace DZALT.Entities.Selection.KillsByPlayer
{
	public record KillsByPlayerQuery : IRequest<KillsByPlayerResult[]>
	{
		private KillsByPlayerQuery()
		{
		}

		public DateTime? From { get; set; }
		public DateTime? To { get; set; }

		public static KillsByPlayerQuery Create(
			DateTime? from = null,
			DateTime? to = null)
			=> new KillsByPlayerQuery()
			{
				From = from,
				To = to,
			};
	}
}
