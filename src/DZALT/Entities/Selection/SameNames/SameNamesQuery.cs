using System;
using MediatR;

namespace DZALT.Entities.Selection.SameNames
{
	public record SameNamesQuery : IRequest<SameNamesResult[]>
	{
		private SameNamesQuery()
		{
		}

		public DateTime? From { get; set; }
		public DateTime? To { get; set; }

		public static SameNamesQuery Create(
			DateTime? from = null,
			DateTime? to = null)
			=> new SameNamesQuery()
			{
				From = from,
				To = to,
			};
	}
}
