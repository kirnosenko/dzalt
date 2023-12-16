using System;

namespace DZALT.Entities.Selection.PlayerLog
{
	public record PlayerDisconnectedLog : PlayerLog
	{
		private PlayerDisconnectedLog()
		{
		}

		public override string ToString()
		{
			return $"{Date}: {Player} disconnected.";
		}

		public static PlayerDisconnectedLog Create(
			DateTime date,
			string player)
			=> new PlayerDisconnectedLog()
			{
				Date = date,
				Player = player,
			};
	}
}
