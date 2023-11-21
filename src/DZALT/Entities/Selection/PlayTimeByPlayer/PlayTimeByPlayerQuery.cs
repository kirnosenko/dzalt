using System;
using MediatR;

namespace DZALT.Entities.Selection.PlayTimeByPlayer
{
	public record PlayTimeByPlayerQuery : IRequest<PlayTimeByPlayerResult[]>
	{
		private PlayTimeByPlayerQuery()
		{
		}

		public DateTime? From { get; set; }
		public DateTime? To { get; set; }

		public static PlayTimeByPlayerQuery Create()
			=> Create(null, null);

		public static PlayTimeByPlayerQuery Create(
			DateTime? from,
			DateTime? to)
			=> new PlayTimeByPlayerQuery()
			{
				From = from,
				To = to,
			};
	}
}
