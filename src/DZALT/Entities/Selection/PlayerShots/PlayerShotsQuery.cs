using System;
using MediatR;

namespace DZALT.Entities.Selection.PlayerShots
{
	public record PlayerShotsQuery : IRequest<PlayerShotsResult[]>
	{
		private PlayerShotsQuery()
		{
		}

		public string[] Bodyparts { get; set; }
		public DateTime? From { get; set; }
		public DateTime? To { get; set; }

		public static PlayerShotsQuery Create(
			string[] bodyparts = null,
			DateTime? from = null,
			DateTime? to = null)
			=> new PlayerShotsQuery()
			{
				Bodyparts = bodyparts,
				From = from,
				To = to,
			};
	}
}
