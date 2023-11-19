using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace DZALT.Entities.Selection.PlayTimeByPlayer
{
	public class PlayTimeByPlayerHandlerTest : BaseRepositoryTest
	{
		private readonly PlayTimeByPlayerHandler handler;

		public PlayTimeByPlayerHandlerTest()
		{
			handler = new PlayTimeByPlayerHandler(this);
		}

		[Fact]
		public async Task ShouldReturnPlayTimeByPlayer()
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
					Date = new DateTime(2023, 11, 11, 10, 00, 00),
				},
				new SessionLog()
				{
					Player = p2,
					Type = SessionLog.SessionType.CONNECTED,
					Date = new DateTime(2023, 11, 11, 11, 00, 00),
				},
				new SessionLog()
				{
					Player = p2,
					Type = SessionLog.SessionType.DISCONNECTED,
					Date = new DateTime(2023, 11, 11, 12, 30, 30),
				},
				new SessionLog()
				{
					Player = p1,
					Type = SessionLog.SessionType.DISCONNECTED,
					Date = new DateTime(2023, 11, 12, 00, 00, 00),
				},
				new SessionLog()
				{
					Player = p1,
					Type = SessionLog.SessionType.CONNECTED,
					Date = new DateTime(2023, 11, 12, 01, 00, 00),
				},
				new SessionLog()
				{
					Player = p1,
					Type = SessionLog.SessionType.DISCONNECTED,
					Date = new DateTime(2023, 11, 12, 01, 00, 10),
				},
			};
			Add(p1);
			Add(p2);
			AddRange(logs);
			await SubmitChanges();

			var result = await handler.Handle(
				PlayTimeByPlayerQuery.Instance,
				default);

			result.Length.Should().Be(2);
			result.Single(x => x.Name == "p1").Time
				.Should().Be(new TimeSpan(14, 00, 10));
			result.Single(x => x.Name == "p2").Time
				.Should().Be(new TimeSpan(01, 30, 30));
		}
	}
}
