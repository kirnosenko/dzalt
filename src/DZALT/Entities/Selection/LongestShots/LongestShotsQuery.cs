using System;
using MediatR;

namespace DZALT.Entities.Selection.LongestShots
{
	public record LongestShotsQuery : IRequest<LongestShotsResult[]>
	{
		private LongestShotsQuery()
		{
		}

		public string[] Bodyparts { get; set; }
		public DateTime? From { get; set; }
		public DateTime? To { get; set; }

		public static LongestShotsQuery Create(
			string[] bodyparts = null,
			DateTime? from = null,
			DateTime? to = null)
			=> new LongestShotsQuery()
			{
				Bodyparts = bodyparts,
				From = from,
				To = to,
			};
	}
}
