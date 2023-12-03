using MediatR;

namespace DZALT.Entities.Selection.PlayerNames
{
	public record PlayerNamesQuery : IRequest<string[]>
	{
		private PlayerNamesQuery()
		{
		}

		public string PlayerNickOrGuid { get; set; }

		public static PlayerNamesQuery Create(
			string playerNickOrGuid)
			=> new PlayerNamesQuery()
			{
				PlayerNickOrGuid = playerNickOrGuid,
			};
	}
}
