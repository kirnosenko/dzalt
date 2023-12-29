using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace DZALT.Entities.Selection.NamesByPlayer
{
	public class NamesByPlayerHandlerTest : BaseRepositoryTest
	{
		private readonly NamesByPlayerHandler handler;

		public NamesByPlayerHandlerTest()
		{
			handler = new NamesByPlayerHandler(this);
		}

		[Fact]
		public async Task ShouldGetNicknameAndGuidForPlayer()
		{
			var player = new Player()
			{
				Guid = "aaa=",
			};
			var nickname = new Nickname()
			{
				Name = "aaa",
				Player = player,
			};
			Add(player);
			Add(nickname);
			await SubmitChanges();

			var result = await handler.Handle(
				NamesByPlayerQuery.Instance, default);

			result[player.Id].Should().Be(
				Helpers.FormatPlayerName(player.Guid, nickname.Name));
		}

		[Fact]
		public async Task ShouldGetGuidForPlayerWhenNoNickname()
		{
			var player = new Player()
			{
				Guid = "aaa=",
			};
			Add(player);
			await SubmitChanges();

			var result = await handler.Handle(
				NamesByPlayerQuery.Instance, default);

			result[player.Id].Should().Be(
				Helpers.FormatPlayerName(player.Guid));
		}

		[Fact]
		public async Task ShouldGetTheLastNicknameForPlayer()
		{
			var player = new Player()
			{
				Guid = "aaa=",
			};
			var nickname = new Nickname()
			{
				Name = "aaa",
				Player = player,
			};
			Add(player);
			Add(nickname);
			await SubmitChanges();
			var nickname2 = new Nickname()
			{
				Name = "bbb",
				Player = player,
			};
			Add(nickname2);
			await SubmitChanges();

			var result = await handler.Handle(
				NamesByPlayerQuery.Instance, default);

			result[player.Id].Should().Be(
				Helpers.FormatPlayerName(player.Guid, nickname2.Name));
		}
	}
}
