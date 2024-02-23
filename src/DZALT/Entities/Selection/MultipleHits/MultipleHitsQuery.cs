using System;
using MediatR;

namespace DZALT.Entities.Selection.MultipleHits
{
	public record MultipleHitsQuery : IRequest<MultipleHitsResult[]>
	{
		private MultipleHitsQuery()
		{
		}

		public DateTime? From { get; set; }
		public DateTime? To { get; set; }
		public string PlayerNickOrGuid { get; init; }
		public bool InvalidOnly { get; set; }

		public static MultipleHitsQuery Create(
			DateTime? from = null,
			DateTime? to = null,
			string playerNickOrGuid = null,
			bool invalidOnly = false)
			=> new MultipleHitsQuery()
			{
				From = from,
				To = to,
				PlayerNickOrGuid = playerNickOrGuid,
				InvalidOnly = invalidOnly,
			};
	}
}
