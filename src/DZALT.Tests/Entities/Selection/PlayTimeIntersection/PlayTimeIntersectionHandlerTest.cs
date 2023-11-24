using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace DZALT.Entities.Selection.PlayTimeIntersection
{
	public class PlayTimeIntersectionHandlerTest : BaseRepositoryTest
	{
		private readonly PlayTimeIntersectionHandler handler;

		public PlayTimeIntersectionHandlerTest()
		{
			handler = new PlayTimeIntersectionHandler(this);
		}

		[Theory]
		[InlineData(10, 12, 12, 14, 0)]
		[InlineData(10, 12, 10, 12, 1)]
		[InlineData(10, 12, 11, 13, 0.5)]
		public async Task ShouldReturnPlayTimeIntersection(
			int p1c, int p1d, int p2c, int p2d, decimal result)
		{
			var p1 = new Player()
			{
				Guid = "p1",
			};
			var p2 = new Player()
			{
				Guid = "p2",
			};
			var logs = new SessionLog[]
			{
				new SessionLog()
				{
					Player = p1,
					Type = SessionLog.SessionType.CONNECTED,
					Date = new DateTime(2023, 11, 11, p1c, 00, 00),
				},
				new SessionLog()
				{
					Player = p1,
					Type = SessionLog.SessionType.DISCONNECTED,
					Date = new DateTime(2023, 11, 11, p1d, 00, 00),
				},
				new SessionLog()
				{
					Player = p2,
					Type = SessionLog.SessionType.CONNECTED,
					Date = new DateTime(2023, 11, 11, p2c, 00, 00),
				},
				new SessionLog()
				{
					Player = p2,
					Type = SessionLog.SessionType.DISCONNECTED,
					Date = new DateTime(2023, 11, 11, p2d, 00, 00),
				},
			};
			Add(p1);
			Add(p2);
			AddRange(logs);
			await SubmitChanges();

			(await handler.Handle(
				PlayTimeIntersectionQuery.Create("p1", "p2", null, null),
				default)).Should().Be(result);
		}
	}
}
