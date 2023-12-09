using System;

namespace DZALT.Entities.Selection.KillsPerHourByPlayer
{
	public record KillsPerHourByPlayerResult
	{
		public string Name { get; set; }
		public decimal Kills { get; set; }
		public TimeSpan Time { get; set; }
		public decimal KillsPerHour { get; set; }
	}
}
