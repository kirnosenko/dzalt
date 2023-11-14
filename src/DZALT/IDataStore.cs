using System;

namespace DZALT
{
	public static class DataStoreExtension
	{
		public static Result UsingSession<Result>(this IDataStore data, Func<ISession,Result> func)
		{
			using (var session = data.OpenSession())
			{
				return func(session);
			}
		}

		public static void UsingSession(this IDataStore data, Action<ISession> action)
		{
			using (var session = data.OpenSession())
			{
				action(session);
			}
		}
	}

	public interface IDataStore
	{
		ISession OpenSession();
	}
}
