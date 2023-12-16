using System;

namespace DZALT.Entities.Selection.PlayerLog
{
	public record PlayerAccidentLog : PlayerLog
	{
		private PlayerAccidentLog()
		{
		}

		public int PlayerX { get; init; }
		public int PlayerY { get; init; }
		public int PlayerZ { get; init; }
		public string Enemy { get; init; }

		public override string ToString()
		{
			string playerPos = $"<{PlayerX},{PlayerY},{PlayerZ}>";
			
			return $"{Date}: {Player} {playerPos} accidently killed by {Enemy}.";
		}

		public static PlayerAccidentLog Create(
			DateTime date,
			string player,
			int playerX,
			int playerY,
			int playerZ,
			string enemy)
			=> new PlayerAccidentLog()
			{
				Date = date,
				Player = player,
				PlayerX = playerX,
				PlayerY = playerY,
				PlayerZ = playerZ,
				Enemy = enemy,
			};
	}
}
