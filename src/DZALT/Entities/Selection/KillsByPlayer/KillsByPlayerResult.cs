namespace DZALT.Entities.Selection.KillsByPlayer
{
	public record KillsByPlayerResult
	{
		public int PlayerId { get; set; }
		public int Kills { get; set; }
		public int Deaths { get; set; }
	}
}
