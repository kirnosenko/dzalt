namespace DZALT.Entities.Selection.PlayersWithMultipleNames
{
	public record PlayersWithMultipleNamesResult
	{
		public int PlayerId { get; set; }
		public string[] Names { get; set; }
	}
}
