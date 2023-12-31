using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace DZALT.Entities.Selection.KillsByPlayer
{
	public class KillsByPlayerHandlerTest : BaseRepositoryTest
	{
		private readonly KillsByPlayerHandler handler;

		public KillsByPlayerHandlerTest()
		{
			handler = new KillsByPlayerHandler(this);
		}

		[Fact]
		public async Task ShouldReturnKillsByPlayer()
		{
			var p1 = new Player()
			{
				Guid = "p1",
			};
			var p2 = new Player()
			{
				Guid = "p2",
			};
			var logs = new EventLog[]
			{
				new EventLog()
				{
					Player = p1,
					EnemyPlayer = p2,
					Event = EventLog.EventType.HIT,
					Date = new DateTime(2023, 11, 12, 10, 00, 00),
				},
				new EventLog()
				{
					Player = p1,
					Event = EventLog.EventType.UNCONSCIOUS,
					Date = new DateTime(2023, 11, 12, 10, 00, 01),
				},
				new EventLog()
				{
					Player = p1,
					EnemyPlayer = p2,
					Event = EventLog.EventType.MURDER,
					Date = new DateTime(2023, 11, 12, 10, 00, 02),
				},
				new EventLog()
				{
					Player = p2,
					EnemyPlayer = p1,
					Event = EventLog.EventType.MURDER,
					Date = new DateTime(2023, 11, 12, 11, 00, 00),
				},
				new EventLog()
				{
					Player = p2,
					EnemyPlayer = p1,
					Event = EventLog.EventType.MURDER,
					Date = new DateTime(2023, 11, 12, 12, 00, 00),
				},
			};
			Add(p1);
			Add(p2);
			AddRange(logs);
			await SubmitChanges();

			var result = await handler.Handle(
				KillsByPlayerQuery.Create(),
				default);

			result.Length.Should().Be(2);
			result.Single(x => x.Name == Helpers.FormatPlayerName(p1.Guid)).Kills
				.Should().Be(2);
			result.Single(x => x.Name == Helpers.FormatPlayerName(p2.Guid)).Kills
				.Should().Be(1);
		}
	}
}
