using System;

namespace DZALT.Entities.Selection.PlayerShots
{
	public record PlayerShotsResult
	{
		public DateTime Date { get; set; }
		public int AttackerId { get; set; }
		public int VictimId { get; set; }
		public string Weapon { get; set; }
		public string Bodypart { get; set; }
		public decimal Distance { get; set; }
	}
}
