using System;

namespace DZALT.Entities.Selection.PlayerLog
{
	public abstract record PlayerLog
	{
		public DateTime Date { get; init; }
		public string Player { get; init; }

		public bool Is<T>(out T log) where T : PlayerLog
		{
			log = this as T;
			return log != null;
		}
	}
}
