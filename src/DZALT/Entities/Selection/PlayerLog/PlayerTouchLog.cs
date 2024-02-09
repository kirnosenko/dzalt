namespace DZALT.Entities.Selection.PlayerLog
{
	public abstract record PlayerTouchLog : PlayerLog
	{
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
		protected string Verb { get; init; }

		public override string ToString()
		{
			string victim = Player;
			string victimPos = $"<{PlayerX},{PlayerY},{PlayerZ}>";
			string attacker = Enemy;
			string attackerPos = $"<{EnemyX},{EnemyY},{EnemyZ}>";
			var distance = Distance == null ? "" : $" from {Distance} meters";
			var into = Bodypart == null ? "" : $" into {Bodypart}";

			return $"{Date}: {victim} {victimPos} {Verb} by {attacker} {attackerPos} with {Weapon}{distance}{into}.";
		}
	}
}
