using System;
using MediatR;

namespace DZALT.Entities.Selection.PlayersWithMultipleNames
{
	public record PlayersWithMultipleNamesQuery : IRequest<PlayersWithMultipleNamesResult[]>
	{
		private PlayersWithMultipleNamesQuery()
		{
		}

		public DateTime? From { get; set; }
		public DateTime? To { get; set; }

		public static PlayersWithMultipleNamesQuery Create(
			DateTime? from = null,
			DateTime? to = null)
			=> new PlayersWithMultipleNamesQuery()
			{
				From = from,
				To = to,
			};
	}
}
