using MediatR;

namespace DZALT.Entities.Selection.PlayerRawEvents
{
	public record PlayerRawEventsQuery : IRequest<EventLog[]>
	{
		private PlayerRawEventsQuery()
		{
		}

		public string PlayerNickOrGuid { get; init; }
		
		public static PlayerRawEventsQuery Create(
			string playerNickOrGuid)
			=> new PlayerRawEventsQuery()
			{
				PlayerNickOrGuid = playerNickOrGuid,
			};
	}
}
