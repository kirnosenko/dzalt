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
		public bool InvalidOnly { get; set; }

		public static MultipleHitsQuery Create(
			DateTime? from = null,
			DateTime? to = null,
			bool invalidOnly = false)
			=> new MultipleHitsQuery()
			{
				From = from,
				To = to,
				InvalidOnly = invalidOnly,
			};
	}
}
