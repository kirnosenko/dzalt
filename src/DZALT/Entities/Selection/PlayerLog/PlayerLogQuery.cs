using MediatR;

namespace DZALT.Entities.Selection.PlayerLog
{
	public record PlayerLogQuery : IRequest<PlayerLog[]>
	{
		private PlayerLogQuery()
		{
		}

		public string PlayerNickOrGuid { get; init; }
		public bool IncludeSessions { get; init; }
		public bool IncludeKills { get; init; }
		public bool IncludeSuicides { get; init; }
		public bool IncludeAccidents { get; init; }

		public static PlayerLogQuery Create(
			string playerNickOrGuid,
			bool includeSessions = true,
			bool includeKills = true,
			bool includeSuicides = true,
			bool includeAccidents = true)
			=> new PlayerLogQuery()
			{
				PlayerNickOrGuid = playerNickOrGuid,
				IncludeSessions = includeSessions,
				IncludeKills = includeKills,
				IncludeSuicides = includeSuicides,
				IncludeAccidents = includeAccidents,
			};
	}
}
