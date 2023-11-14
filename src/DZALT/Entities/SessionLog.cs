using System;

namespace DZALT.Entities
{
	public class SessionLog : Log
	{
		public enum Reason
		{
			CONNECTED = 0,
			DISCONNECTED = 1,
		}

		public Reason Type { get; set; }
	}
}
