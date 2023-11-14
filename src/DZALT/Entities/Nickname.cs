namespace DZALT.Entities
{
	public class Nickname
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int PlayerId { get; set; }

		public Player Player { get; set; }
	}
}
