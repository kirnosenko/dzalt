using System;
using MediatR;

namespace DZALT.Entities.Selection.Weapons
{
	public record WeaponsQuery : IRequest<WeaponsResult[]>
	{
		private WeaponsQuery()
		{
		}

		public DateTime? From { get; set; }
		public DateTime? To { get; set; }
		public bool ExcludeMelee { get; set; }
		public bool ExcludeDistance { get; set; }

		public static WeaponsQuery Create(
			DateTime? from = null,
			DateTime? to = null,
			bool excludeMelee = false,
			bool excludeDistance = false)
			=> new WeaponsQuery()
			{
				From = from,
				To = to,
				ExcludeMelee = excludeMelee,
				ExcludeDistance = excludeDistance,
			};
	}
}
