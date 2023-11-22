using System;
using System.Linq;
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
		public async Task ShouldGetNicknameForPlayer()
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

			result[player.Id].Should().Be(nickname.Name);
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

			result[player.Id].Should().Be(player.Guid);
		}
	}
}
