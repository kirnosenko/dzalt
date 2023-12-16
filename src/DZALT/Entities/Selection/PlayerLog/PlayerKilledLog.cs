using System;

namespace DZALT.Entities.Selection.PlayerLog
{
	public record PlayerKilledLog : PlayerLog
	{
		private PlayerKilledLog()
		{
		}

		public int PlayerX { get; init; }
		public int PlayerY { get; init; }
		public int PlayerZ { get; init; }
		public string Enemy { get; init; }
		public int EnemyX { get; init; }
		public int EnemyY { get; init; }
		public int EnemyZ { get; init; }
		public string Weapon { get; init; }
		public int? Distance { get; init; }
		public string Bodypart { get; init; }

		public override string ToString()
		{
			string victim = Player;
			string victimPos = $"<{PlayerX},{PlayerY},{PlayerZ}>";
			string attacker = Enemy;
			string attackerPos = $"<{EnemyX},{EnemyY},{EnemyZ}>";
			var distance = Distance == null ? "" : $" from {Distance} meters";
			var into = Bodypart == null ? "" : $" into {Bodypart}";
			
			return $"{Date}: {victim} {victimPos} killed by {attacker} {attackerPos} with {Weapon}{distance}{into}.";
		}

		public static PlayerKilledLog Create(
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
			=> new PlayerKilledLog()
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
