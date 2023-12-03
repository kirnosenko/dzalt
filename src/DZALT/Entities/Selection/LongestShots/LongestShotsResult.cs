using System;

namespace DZALT.Entities.Selection.LongestShots
{
	public record LongestShotsResult
	{
		public DateTime Date { get; set; }
		public string Attacker { get; set; }
		public string Victim { get; set; }
		public string Weapon { get; set; }
		public string Bodypart { get; set; }
		public decimal Distance { get; set; }
	}
}
