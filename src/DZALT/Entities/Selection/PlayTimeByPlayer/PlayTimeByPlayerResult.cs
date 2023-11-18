using System;

namespace DZALT.Entities.Selection.PlayTimeByPlayer
{
	public record PlayTimeByPlayerResult
	{
		public string Name { get; set; }
		public TimeSpan Time { get; set; }
	}
}
