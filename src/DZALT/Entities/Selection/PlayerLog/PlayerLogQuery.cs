using System;
using MediatR;

namespace DZALT.Entities.Selection.PlayerLog
{
	public record PlayerLogQuery : IRequest<PlayerLog[]>
	{
		private PlayerLogQuery()
		{
		}

		public DateTime? From { get; set; }
		public DateTime? To { get; set; }
		public string PlayerNickOrGuid { get; init; }
		public bool IncludeSessions { get; init; }
		public bool IncludeHits { get; init; }
		public bool IncludeKills { get; init; }
		public bool IncludeSuicides { get; init; }
		public bool IncludeAccidents { get; init; }

		public static PlayerLogQuery Create(
			DateTime? from,
			DateTime? to,
			string playerNickOrGuid,
			bool includeSessions = true,
			bool includeHits = true,
			bool includeKills = true,
			bool includeSuicides = true,
			bool includeAccidents = true)
			=> new PlayerLogQuery()
			{
				From = from,
				To = to,
				PlayerNickOrGuid = playerNickOrGuid,
				IncludeSessions = includeSessions,
				IncludeHits = includeHits,
				IncludeKills = includeKills,
				IncludeSuicides = includeSuicides,
				IncludeAccidents = includeAccidents,
			};
	}
}
