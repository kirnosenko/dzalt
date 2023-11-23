using MediatR;

namespace DZALT.Entities.Selection.PlayerLog
{
	public record PlayerLogQuery : IRequest<string[]>
	{
		private PlayerLogQuery()
		{
		}

		public string Nickname { get; init; }

		public static PlayerLogQuery Create(
			string nickname)
			=> new PlayerLogQuery()
			{
				Nickname = nickname
			};
	}
}
