namespace DZALT.Entities.Selection.SameNames
{
	public record SameNamesResult
	{
		public string Name { get; set; }
		public string[] Guids { get; set; }
	}
}
