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
			var p1 = new Player();
			var p2 = new Player();
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
				PlayTimeByPlayerQuery.Create(),
				default);

			result.Length.Should().Be(2);
			result.Single(x => x.PlayerId == p1.Id).Time
				.Should().Be(new TimeSpan(14, 00, 10));
			result.Single(x => x.PlayerId == p2.Id).Time
				.Should().Be(new TimeSpan(01, 30, 30));
		}

		[Theory]
		[InlineData(null, null, 7)]
		[InlineData(null, 13, 5)]
		[InlineData(null, 12, 1)]
		[InlineData(11, null, 7)]
		[InlineData(12, null, 6)]
		[InlineData(13, null, 2)]
		[InlineData(12, 13, 4)]
		public async Task ShouldReturnPlayTimeByPlayerWithinDateRange(
			int? from, int? to, int hours)
		{
			DateTime? fromDate = from == null ? null : new DateTime(2023, 11, from.Value);
			DateTime? toDate = to == null ? null : new DateTime(2023, 11, to.Value);

			var p1 = new Player();
			var logs = new SessionLog[]
			{
				new SessionLog()
				{
					Player = p1,
					Type = SessionLog.SessionType.CONNECTED,
					Date = new DateTime(2023, 11, 11, 23, 00, 00),
				},
				new SessionLog()
				{
					Player = p1,
					Type = SessionLog.SessionType.DISCONNECTED,
					Date = new DateTime(2023, 11, 12, 01, 00, 00),
				},
				new SessionLog()
				{
					Player = p1,
					Type = SessionLog.SessionType.CONNECTED,
					Date = new DateTime(2023, 11, 12, 10, 00, 00),
				},
				new SessionLog()
				{
					Player = p1,
					Type = SessionLog.SessionType.DISCONNECTED,
					Date = new DateTime(2023, 11, 12, 11, 00, 00),
				},
				new SessionLog()
				{
					Player = p1,
					Type = SessionLog.SessionType.CONNECTED,
					Date = new DateTime(2023, 11, 12, 22, 00, 00),
				},
				new SessionLog()
				{
					Player = p1,
					Type = SessionLog.SessionType.DISCONNECTED,
					Date = new DateTime(2023, 11, 13, 02, 00, 00),
				},
			};
			Add(p1);
			AddRange(logs);
			await SubmitChanges();

			var result = await handler.Handle(
				PlayTimeByPlayerQuery.Create(fromDate, toDate),
				default);

			result.Length.Should().Be(1);
			result.Single(x => x.PlayerId == p1.Id).Time
				.Should().Be(TimeSpan.FromHours(hours));
		}
	}
}
