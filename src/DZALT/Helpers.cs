namespace DZALT
{
	public static class Helpers
	{
		public static string FormatPlayerName(string guid, string nickname = null)
		{
			guid = guid.Length == 44 ? guid.Substring(44 - 1 - 8) : guid;
			nickname = nickname != null
				? $"{nickname} "
				: "";

			return $"{nickname}({guid})";
		}
	}
}
