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

		public static PlayTimeByPlayerQuery Create(
			DateTime? from = null,
			DateTime? to = null)
			=> new PlayTimeByPlayerQuery()
			{
				From = from,
				To = to,
			};
	}
}
