using System;

namespace DZALT.Entities.Selection.TimeTillFirstKill
{
	public record TimeTillFirstKillResult
	{
		public int PlayerId { get; set; }
		public DateTime DeathDate { get; set; }
		public DateTime KillDate { get; set; }
		public TimeSpan Time { get; set; }
	}
}
