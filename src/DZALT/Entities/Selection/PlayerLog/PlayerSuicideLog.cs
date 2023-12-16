using System;

namespace DZALT.Entities.Selection.PlayerLog
{
	public record PlayerSuicideLog : PlayerLog
	{
		private PlayerSuicideLog()
		{
		}

		public int PlayerX { get; init; }
		public int PlayerY { get; init; }
		public int PlayerZ { get; init; }

		public override string ToString()
		{
			string playerPos = $"<{PlayerX},{PlayerY},{PlayerZ}>";

			return $"{Date}: {Player} {playerPos} committed suicide.";
		}

		public static PlayerSuicideLog Create(
			DateTime date,
			string player,
			int playerX,
			int playerY,
			int playerZ)
			=> new PlayerSuicideLog()
			{
				Date = date,
				Player = player,
				PlayerX = playerX,
				PlayerY = playerY,
				PlayerZ = playerZ,
			};
	}
}
