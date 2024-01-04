using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Selection.PlayerNames
{
	public record PlayerNamesHandler : IRequestHandler<PlayerNamesQuery, string[]>
	{
		private readonly IRepository repository;

		public PlayerNamesHandler(IRepository repository)
		{
			this.repository = repository;
		}

		public async Task<string[]> Handle(
			PlayerNamesQuery query,
			CancellationToken cancellationToken)
		{
			var playerName = query.PlayerNickOrGuid;
			var playerId = await repository.PlayerIdByName(playerName, cancellationToken);
			var playerGuid = await repository.Get<Player>()
				.Where(x => x.Id == playerId)
				.Select(x => x.Guid)
				.FirstOrDefaultAsync(cancellationToken);

			var nicknames = await repository.Get<Nickname>()
				.Where(x => x.PlayerId == playerId)
				.Select(x => x.Name)
				.ToArrayAsync(cancellationToken);

			return new string[] { playerGuid }.Concat(nicknames).ToArray();
		}
	}
}
