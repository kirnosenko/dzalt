using System;

namespace DZALT.Entities.Selection.PlayTimeByPlayer
{
	public record PlayTimeByPlayerResult
	{
		public int PlayerId { get; set; }
		public TimeSpan Time { get; set; }
	}
}
