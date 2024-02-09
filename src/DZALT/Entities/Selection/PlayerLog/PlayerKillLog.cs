using System;

namespace DZALT.Entities.Selection.PlayerLog
{
	public record PlayerKillLog : PlayerTouchLog
	{
		private PlayerKillLog()
		{
			Verb = "killed";
		}

		public override string ToString()
		{
			return base.ToString();
		}

		public static PlayerKillLog Create(
			DateTime date,
			string player,
			int playerX,
			int playerY,
			int playerZ,
			string enemy,
			int enemyX,
			int enemyY,
			int enemyZ,
			string weapon,
			int? distance,
			string bodypart)
			=> new PlayerKillLog()
			{
				Date = date,
				Player = player,
				PlayerX = playerX,
				PlayerY = playerY,
				PlayerZ = playerZ,
				Enemy = enemy,
				EnemyX = enemyX,
				EnemyY = enemyY,
				EnemyZ = enemyZ,
				Weapon = weapon,
				Distance = distance,
				Bodypart = bodypart,
			};
	}
}
