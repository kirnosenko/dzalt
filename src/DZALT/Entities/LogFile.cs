using System;

namespace DZALT.Entities
{
	public class LogFile
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public DateTime DateFrom { get; set; }
		public DateTime DateTo { get; set; }
	}
}
