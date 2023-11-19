namespace DZALT.Entities
{
	public class SessionLog : Log
	{
		public enum SessionType
		{
			CONNECTED = 0,
			DISCONNECTED = 1,
		}

		public SessionType Type { get; set; }
	}
}
