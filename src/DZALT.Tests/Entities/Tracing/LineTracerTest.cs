using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DZALT.Entities;
using DZALT.Entities.Tracing;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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

		[Fact]
		public async Task ShouldNotAddUnknownPlayer()
		{
			var log = await tracer.Trace(
				$"11:23:45 | Player \"Survivor\"(id=Unknown) has been disconnected",
				CancellationToken.None);
			await SubmitChanges();

			log.Should().BeNull();
			(await Get<Player>().SingleOrDefaultAsync()).Should().BeNull();
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
			sessionLogs.Where(x => x.Type == SessionLog.SessionType.CONNECTED).Count().Should().Be(2);
			sessionLogs.Where(x => x.Type == SessionLog.SessionType.DISCONNECTED).Count().Should().Be(2);
			sessionLogs.Select(x => x.Date).Should().BeEquivalentTo(new DateTime[]
			{
				new DateTime(1, 1, 1, 11, 23, 45),
				new DateTime(1, 1, 1, 11, 23, 46),
				new DateTime(1, 1, 1, 12, 23, 46),
				new DateTime(1, 1, 1, 13, 23, 45),
			});
		}

		[Fact]
		public async Task ShouldAddEventForHitByPlayer()
		{
			await tracer.Trace(
				@"10:11:12 | Player ""aaa"" (id=aaa= pos=<10011, 5461.9, 253.6>)[HP: 2.89747] hit by Player ""bbb"" (id=bbb= pos=<10013.7, 5466.8, 253.6>) into Head(0) for 8.68342 damage (Bullet_380) with CR-61 Skorpion from 5.64337 meters",
				CancellationToken.None);
			await SubmitChanges();

			var player = await Get<Player>().SingleOrDefaultAsync(x => x.Guid == "aaa=");
			var enemy = await Get<Player>().SingleOrDefaultAsync(x => x.Guid == "bbb=");
			var eventLog = await Get<EventLog>().SingleOrDefaultAsync();

			player.Should().NotBeNull();
			enemy.Should().NotBeNull();
			eventLog.Should().NotBeNull();
			eventLog.PlayerId.Should().Be(player.Id);
			eventLog.Date.Should().Be(new DateTime(1, 1, 1, 10, 11, 12));
			eventLog.X.Should().Be(10011);
			eventLog.Y.Should().Be(5461.9M);
			eventLog.Z.Should().Be(253.6M);
			eventLog.Event.Should().Be(EventLog.EventType.HIT);
			eventLog.EnemyPlayerId.Should().Be(enemy.Id);
			eventLog.EnemyPlayerX.Should().Be(10013.7M);
			eventLog.EnemyPlayerY.Should().Be(5466.8M);
			eventLog.EnemyPlayerZ.Should().Be(253.6M);
			eventLog.Damage.Should().Be(8.68342M);
			eventLog.Health.Should().Be(2.89747M);
			eventLog.Enemy.Should().BeNull();
			eventLog.BodyPart.Should().Be("Head(0)");
			eventLog.Hitter.Should().Be("Bullet_380");
			eventLog.Weapon.Should().Be("CR-61 Skorpion");
			eventLog.Distance.Should().Be(5.64337M);
		}

		[Fact]
		public async Task ShouldAddEventForHitByZombie()
		{
			await tracer.Trace(
				@"10:11:12 | Player ""abc"" (id=abc= pos=<14290, 13280.9, 3.3>)[HP: 93.5] hit by Infected into Torso(1) for 6.5 damage (MeleeInfected)",
				CancellationToken.None);
			await SubmitChanges();

			var player = await Get<Player>().SingleOrDefaultAsync();
			var eventLog = await Get<EventLog>().SingleOrDefaultAsync();

			player.Should().NotBeNull();
			eventLog.Should().NotBeNull();
			eventLog.PlayerId.Should().Be(player.Id);
			eventLog.Date.Should().Be(new DateTime(1, 1, 1, 10, 11, 12));
			eventLog.X.Should().Be(14290);
			eventLog.Y.Should().Be(13280.9M);
			eventLog.Z.Should().Be(3.3M);
			eventLog.Event.Should().Be(EventLog.EventType.HIT);
			eventLog.Damage.Should().Be(6.5M);
			eventLog.Health.Should().Be(93.5M);
			eventLog.Enemy.Should().Be("Infected");
			eventLog.BodyPart.Should().Be("Torso(1)");
			eventLog.Hitter.Should().Be("MeleeInfected");
			eventLog.Weapon.Should().BeNull();
			eventLog.Distance.Should().BeNull();
		}

		[Fact]
		public async Task ShouldAddEventForKillByPlayerWithMelee()
		{
			await tracer.Trace(
				@"10:11:12 | Player ""aaa"" (DEAD) (id=aaa= pos=<13387.1, 6314.0, 6.6>) killed by Player ""bbb"" (id=bbb= pos=<13386.9, 6316.0, 6.6>) with Hunting knife",
				CancellationToken.None);
			await SubmitChanges();

			var player = await Get<Player>().SingleOrDefaultAsync(x => x.Guid == "aaa=");
			var enemy = await Get<Player>().SingleOrDefaultAsync(x => x.Guid == "bbb=");
			var eventLog = await Get<EventLog>().SingleOrDefaultAsync();

			player.Should().NotBeNull();
			enemy.Should().NotBeNull();
			eventLog.Should().NotBeNull();
			eventLog.PlayerId.Should().Be(player.Id);
			eventLog.Date.Should().Be(new DateTime(1, 1, 1, 10, 11, 12));
			eventLog.X.Should().Be(13387.1M);
			eventLog.Y.Should().Be(6314.0M);
			eventLog.Z.Should().Be(6.6M);
			eventLog.Event.Should().Be(EventLog.EventType.MURDER);
			eventLog.EnemyPlayerId.Should().Be(enemy.Id);
			eventLog.EnemyPlayerX.Should().Be(13386.9M);
			eventLog.EnemyPlayerY.Should().Be(6316.0M);
			eventLog.EnemyPlayerZ.Should().Be(6.6M);
			eventLog.Damage.Should().BeNull();
			eventLog.Health.Should().BeNull();
			eventLog.Enemy.Should().BeNull();
			eventLog.BodyPart.Should().BeNull();
			eventLog.Hitter.Should().BeNull();
			eventLog.Weapon.Should().Be("Hunting knife");
			eventLog.Distance.Should().BeNull();
		}

		[Fact]
		public async Task ShouldAddEventForKillByPlayerWithGun()
		{
			await tracer.Trace(
				@"10:11:12 | Player ""aaa"" (DEAD) (id=aaa= pos=<10011, 5461.9, 253.6>) killed by Player ""bbb"" (id=bbb= pos=<10013.6, 5466.6, 253.6>) with CR-61 Skorpion from 5.43375 meters",
				CancellationToken.None);
			await SubmitChanges();

			var player = await Get<Player>().SingleOrDefaultAsync(x => x.Guid == "aaa=");
			var enemy = await Get<Player>().SingleOrDefaultAsync(x => x.Guid == "bbb=");
			var eventLog = await Get<EventLog>().SingleOrDefaultAsync();

			player.Should().NotBeNull();
			enemy.Should().NotBeNull();
			eventLog.Should().NotBeNull();
			eventLog.PlayerId.Should().Be(player.Id);
			eventLog.Date.Should().Be(new DateTime(1, 1, 1, 10, 11, 12));
			eventLog.X.Should().Be(10011M);
			eventLog.Y.Should().Be(5461.9M);
			eventLog.Z.Should().Be(253.6M);
			eventLog.Event.Should().Be(EventLog.EventType.MURDER);
			eventLog.EnemyPlayerId.Should().Be(enemy.Id);
			eventLog.EnemyPlayerX.Should().Be(10013.6M);
			eventLog.EnemyPlayerY.Should().Be(5466.6M);
			eventLog.EnemyPlayerZ.Should().Be(253.6M);
			eventLog.Damage.Should().BeNull();
			eventLog.Health.Should().BeNull();
			eventLog.Enemy.Should().BeNull();
			eventLog.BodyPart.Should().BeNull();
			eventLog.Hitter.Should().BeNull();
			eventLog.Weapon.Should().Be("CR-61 Skorpion");
			eventLog.Distance.Should().Be(5.43375M);
		}

		[Fact]
		public async Task ShouldAddEventForKillByZombie()
		{
			await tracer.Trace(
				@"10:11:12 | Player ""aaa"" (DEAD) (id=aaa= pos=<5188.9, 2170.9, 8.9>) killed by ZmbM_PatrolNormal_Autumn",
				CancellationToken.None);
			await SubmitChanges();

			var player = await Get<Player>().SingleOrDefaultAsync();
			var eventLog = await Get<EventLog>().SingleOrDefaultAsync();

			player.Should().NotBeNull();
			eventLog.Should().NotBeNull();
			eventLog.PlayerId.Should().Be(player.Id);
			eventLog.Date.Should().Be(new DateTime(1, 1, 1, 10, 11, 12));
			eventLog.X.Should().Be(5188.9M);
			eventLog.Y.Should().Be(2170.9M);
			eventLog.Z.Should().Be(8.9M);
			eventLog.Event.Should().Be(EventLog.EventType.ACCIDENT);
			eventLog.Damage.Should().BeNull();
			eventLog.Health.Should().BeNull();
			eventLog.Enemy.Should().Be("ZmbM_PatrolNormal_Autumn");
			eventLog.BodyPart.Should().BeNull();
			eventLog.Hitter.Should().BeNull();
			eventLog.Weapon.Should().BeNull();
			eventLog.Distance.Should().BeNull();
		}

		[Fact]
		public async Task ShouldAddEventForHitWhenPlayerDead()
		{
			await tracer.Trace(
				@"10:11:12 | Player ""aaa"" (DEAD) (id=aaa= pos=<5188.9, 2170.9, 8.9>)[HP: 0] hit by Infected into LeftLeg(8) for 7.225 damage (MeleeSoldierInfected)",
				CancellationToken.None);
			await SubmitChanges();

			var player = await Get<Player>().SingleOrDefaultAsync();
			var eventLog = await Get<EventLog>().SingleOrDefaultAsync();

			player.Should().NotBeNull();
			eventLog.Should().NotBeNull();
			eventLog.PlayerId.Should().Be(player.Id);
			eventLog.Date.Should().Be(new DateTime(1, 1, 1, 10, 11, 12));
			eventLog.X.Should().Be(5188.9M);
			eventLog.Y.Should().Be(2170.9M);
			eventLog.Z.Should().Be(8.9M);
			eventLog.Event.Should().Be(EventLog.EventType.HIT);
			eventLog.Damage.Should().Be(7.225M);
			eventLog.Health.Should().Be(0);
			eventLog.Enemy.Should().Be("Infected");
			eventLog.BodyPart.Should().Be("LeftLeg(8)");
			eventLog.Hitter.Should().Be("MeleeSoldierInfected");
			eventLog.Weapon.Should().BeNull();
			eventLog.Distance.Should().BeNull();
		}

		[Fact]
		public async Task ShouldAddEventForUnconscious()
		{
			await tracer.Trace(
				@"10:11:12 | Player ""aaa"" (id=aaa= pos=<11486.2, 14481.9, 58.1>) is unconscious",
				CancellationToken.None);
			await SubmitChanges();

			var player = await Get<Player>().SingleOrDefaultAsync();
			var eventLog = await Get<EventLog>().SingleOrDefaultAsync();

			player.Should().NotBeNull();
			eventLog.Should().NotBeNull();
			eventLog.PlayerId.Should().Be(player.Id);
			eventLog.Date.Should().Be(new DateTime(1, 1, 1, 10, 11, 12));
			eventLog.X.Should().Be(11486.2M);
			eventLog.Y.Should().Be(14481.9M);
			eventLog.Z.Should().Be(58.1M);
			eventLog.Event.Should().Be(EventLog.EventType.UNCONSCIOUS);
			eventLog.Damage.Should().BeNull();
			eventLog.Health.Should().BeNull();
			eventLog.Enemy.Should().BeNull();
			eventLog.BodyPart.Should().BeNull();
			eventLog.Hitter.Should().BeNull();
			eventLog.Weapon.Should().BeNull();
			eventLog.Distance.Should().BeNull();
		}

		[Fact]
		public async Task ShouldAddEventForRegainedConsciousness()
		{
			await tracer.Trace(
				@"10:11:12 | Player ""aaa"" (id=aaa= pos=<10502.7, 6068.5, 261.8>) regained consciousness",
				CancellationToken.None);
			await SubmitChanges();

			var player = await Get<Player>().SingleOrDefaultAsync();
			var eventLog = await Get<EventLog>().SingleOrDefaultAsync();

			player.Should().NotBeNull();
			eventLog.Should().NotBeNull();
			eventLog.PlayerId.Should().Be(player.Id);
			eventLog.Date.Should().Be(new DateTime(1, 1, 1, 10, 11, 12));
			eventLog.X.Should().Be(10502.7M);
			eventLog.Y.Should().Be(6068.5M);
			eventLog.Z.Should().Be(261.8M);
			eventLog.Event.Should().Be(EventLog.EventType.CONSCIOUS);
			eventLog.Damage.Should().BeNull();
			eventLog.Health.Should().BeNull();
			eventLog.Enemy.Should().BeNull();
			eventLog.BodyPart.Should().BeNull();
			eventLog.Hitter.Should().BeNull();
			eventLog.Weapon.Should().BeNull();
			eventLog.Distance.Should().BeNull();
		}

		[Fact]
		public async Task ShouldAddEventForDeadStateWithStats()
		{
			await tracer.Trace(
				@"10:11:12 | Player ""aaa"" (DEAD) (id=aaa= pos=<11492.3, 5503.6, 243.2>) died. Stats> Water: 848.615 Energy: 0 Bleed sources: 3",
				CancellationToken.None);
			await SubmitChanges();

			var player = await Get<Player>().SingleOrDefaultAsync();
			var eventLog = await Get<EventLog>().SingleOrDefaultAsync();

			player.Should().NotBeNull();
			eventLog.Should().NotBeNull();
			eventLog.PlayerId.Should().Be(player.Id);
			eventLog.Date.Should().Be(new DateTime(1, 1, 1, 10, 11, 12));
			eventLog.X.Should().Be(11492.3M);
			eventLog.Y.Should().Be(5503.6M);
			eventLog.Z.Should().Be(243.2M);
			eventLog.Event.Should().Be(EventLog.EventType.ACCIDENT);
			eventLog.Damage.Should().BeNull();
			eventLog.Health.Should().BeNull();
			eventLog.Enemy.Should().BeNull();
			eventLog.BodyPart.Should().BeNull();
			eventLog.Hitter.Should().BeNull();
			eventLog.Weapon.Should().BeNull();
			eventLog.Distance.Should().BeNull();
		}
	}
}
