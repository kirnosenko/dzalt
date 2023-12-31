using System;
using MediatR;

namespace DZALT.Entities.Selection.TouchedPlayers
{
	public record TouchedPlayersQuery : IRequest<TouchedPlayersResult[]>
	{
		private TouchedPlayersQuery()
		{
		}

		public DateTime? From { get; set; }
		public DateTime? To { get; set; }

		public static TouchedPlayersQuery Create(
			DateTime? from = null,
			DateTime? to = null)
			=> new TouchedPlayersQuery()
			{
				From = from,
				To = to,
			};
	}
}
