using System;

namespace DZALT.Entities.Selection.PlayerLog
{
	public record PlayerConnectedLog : PlayerLog
	{
		private PlayerConnectedLog()
		{
		}

		public override string ToString()
		{
			return $"{Date}: {Player} connected.";
		}

		public static PlayerConnectedLog Create(
			DateTime date,
			string player)
			=> new PlayerConnectedLog()
			{
				Date = date,
				Player = player,
			};
	}
}
