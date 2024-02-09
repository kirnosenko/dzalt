using System;

namespace DZALT.Entities.Selection.PlayerLog
{
	public record PlayerDisconnectLog : PlayerLog
	{
		private PlayerDisconnectLog()
		{
		}

		public override string ToString()
		{
			return $"{Date}: {Player} disconnected.";
		}

		public static PlayerDisconnectLog Create(
			DateTime date,
			string player)
			=> new PlayerDisconnectLog()
			{
				Date = date,
				Player = player,
			};
	}
}
