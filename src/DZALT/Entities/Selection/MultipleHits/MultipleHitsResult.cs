using System;

namespace DZALT.Entities.Selection.MultipleHits
{
	public record MultipleHitsResult
	{
		public int PlayerId { get; set; }
		public DateTime Date { get; set; }
		public int Shots { get; set; }
		public string Weapon { get; set; }
		public decimal Distance { get; set; }
	}
}
