namespace DZALT.Entities.Selection.Weapons
{
	public record WeaponsResult
	{
		public string Weapon { get; set; }
		public int Hits { get; set;}
		public int Kills { get; set; }
	}
}
