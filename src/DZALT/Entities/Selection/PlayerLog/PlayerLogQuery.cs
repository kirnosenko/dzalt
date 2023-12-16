using MediatR;

namespace DZALT.Entities.Selection.PlayerLog
{
	public record PlayerLogQuery : IRequest<PlayerLog[]>
	{
		private PlayerLogQuery()
		{
		}

		public string PlayerNickOrGuid { get; set; }


		public static PlayerLogQuery Create(
			string playerNickOrGuid)
			=> new PlayerLogQuery()
			{
				PlayerNickOrGuid = playerNickOrGuid,
			};
	}
}
