using System;

namespace DZALT.Entities.Selection.PlayerLog
{
	public record PlayerConnectLog : PlayerLog
	{
		private PlayerConnectLog()
		{
		}

		public override string ToString()
		{
			return $"{Date}: {Player} connected.";
		}

		public static PlayerConnectLog Create(
			DateTime date,
			string player)
			=> new PlayerConnectLog()
			{
				Date = date,
				Player = player,
			};
	}
}
