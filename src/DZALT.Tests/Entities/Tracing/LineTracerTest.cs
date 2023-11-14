using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DZALT.Entities;
using DZALT.Entities.Tracing;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace DZALT.Tests.Entities.Tracing
{
    public class LineTracerTest : BaseRepositoryTest
    {
        private readonly LineTracer tracer;

        public LineTracerTest()
        {
			tracer = new LineTracer(this);
		}

        [Fact]
		public async Task ShouldAddPlayerAndNickname()
        {
			await tracer.Trace(
				"11:23:45 | Player \"abc\" is connected (id=abc-ABC=)",
				CancellationToken.None);
            await SubmitChanges();

            var player = await Get<Player>().SingleOrDefaultAsync();
            var nickname = await Get<Nickname>().SingleOrDefaultAsync();

			player.Should().NotBeNull();
            player.Guid.Should().Be("abc-ABC=");
            nickname.Should().NotBeNull();
            nickname.Name.Should().Be("abc");
            nickname.PlayerId.Should().Be(player.Id);
		}

		[Fact]
		public async Task ShouldAllowPlayersWithTheSameNickname()
		{
			await tracer.Trace(
				"11:23:45 | Player \"abc\" is connected (id=aaa-AAA=)",
				CancellationToken.None);
			await tracer.Trace(
				"11:23:46 | Player \"abc\" is connected (id=_bbbBBB=)",
				CancellationToken.None);
			await SubmitChanges();

			var players = await Get<Player>().CountAsync();
			var nicknames = await Get<Nickname>().CountAsync();

			players.Should().Be(2);
			nicknames.Should().Be(2);
		}

		[Theory]
		[InlineData("Survivor")]
		[InlineData("Survivor (1)")]
		[InlineData("Survivor (20)")]
		public async Task ShouldNotAddDefaultNickname(string nick)
		{
			await tracer.Trace(
				$"11:23:45 | Player \"{nick}\" is connected (id=abc-ABC=)",
				CancellationToken.None);
			await SubmitChanges();

			var player = await Get<Player>().SingleOrDefaultAsync();
			var nickname = await Get<Nickname>().SingleOrDefaultAsync();

			player.Should().NotBeNull();
			player.Guid.Should().Be("abc-ABC=");
			nickname.Should().BeNull();
		}

        [Fact]
		public async Task ShouldSaveSessions()
        {
            await tracer.Trace(
				"11:23:45 | Player \"abc\" is connected (id=abc-ABC=)",
                CancellationToken.None);
            await tracer.Trace(
				"11:23:46 | Player \"aaa\" is connected (id=_aaaAAA=)",
                CancellationToken.None);
            await tracer.Trace(
				"12:23:46 | Player \"aaa\"(id=_aaaAAA=) has been disconnected",
                CancellationToken.None);
            await tracer.Trace(
				"13:23:45 | Player \"abc\"(id=abc-ABC=) has been disconnected",
                CancellationToken.None);
			await SubmitChanges();

			var sessionLogs = await Get<SessionLog>().ToArrayAsync();

			sessionLogs.Length.Should().Be(4);
			sessionLogs.Where(x => x.Type == SessionLog.Reason.CONNECTED).Count().Should().Be(2);
			sessionLogs.Where(x => x.Type == SessionLog.Reason.DISCONNECTED).Count().Should().Be(2);
			sessionLogs.Select(x => x.Date).Should().BeEquivalentTo(new DateTime[]
			{
				new DateTime(1, 1, 1, 11, 23, 45),
				new DateTime(1, 1, 1, 11, 23, 46),
				new DateTime(1, 1, 1, 12, 23, 46),
				new DateTime(1, 1, 1, 13, 23, 45),
			});
		}
	}
}
