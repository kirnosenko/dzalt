using MediatR;
using System;

namespace DZALT.Entities.Selection.PlayTimeIntersection
{
	public record PlayTimeIntersectionQuery : IRequest<decimal>
	{
		private PlayTimeIntersectionQuery()
		{
		}

		public string P1NickOrGuid { get; set; }
		public string P2NickOrGuid { get; set; }
		public DateTime? From { get; set; }
		public DateTime? To { get; set; }

		public static PlayTimeIntersectionQuery Create(
			string p1NickOrGuid,
			string p2NickOrGuid,
			DateTime? from = null,
			DateTime? to = null)
			=> new PlayTimeIntersectionQuery()
			{
				P1NickOrGuid = p1NickOrGuid,
				P2NickOrGuid = p2NickOrGuid,
				From = from,
				To = to,
			};
	}
}
