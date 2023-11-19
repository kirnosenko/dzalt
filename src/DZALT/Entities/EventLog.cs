namespace DZALT.Entities
{
	public class EventLog : Log
	{
		public enum EventType
		{
			HIT = 0,
			UNCONSCIOUS = 1,
			CONSCIOUS = 2,
			SUICIDE = 10,
			MURDER = 11,
			ACCIDENT = 12,
		}

		public decimal X { get; set; }
		public decimal Y { get; set; }
		public decimal Z { get; set; }

		public int? EnemyPlayerId { get; set; }
		public decimal? EnemyPlayerX { get; set; }
		public decimal? EnemyPlayerY { get; set; }
		public decimal? EnemyPlayerZ { get; set; }

		public EventType Event { get; set; }
		public decimal? Damage { get; set; }
		public decimal? Health { get; set; }
		public string Enemy { get; set; }
		public string BodyPart { get; set; }
		public string Hitter { get; set; }
		public string Weapon { get; set; }
		public decimal? Distance { get; set; }

		public Player EnemyPlayer { get; set; }
	}
}
