using System;

namespace DZALT.Entities
{
	public abstract class Log
	{
		public int Id { get; set; }
		public int? PlayerId { get; set; }
		public DateTime Date { get; set; }

		public Player Player { get; set; }
	}
}
