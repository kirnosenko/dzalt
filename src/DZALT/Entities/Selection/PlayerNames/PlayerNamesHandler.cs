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
			var playerIds = await repository.PlayerIdsByName(playerName, cancellationToken);
			var playerGuids = await repository.Get<Player>()
				.Where(x => playerIds.Any(id => id == x.Id))
				.Select(x => x.Guid)
				.ToArrayAsync(cancellationToken);

			var nicknames = await repository.Get<Nickname>()
				.Where(x => playerIds.Any(id => id == x.PlayerId))
				.Select(x => x.Name)
				.ToArrayAsync(cancellationToken);

			return playerGuids.Concat(nicknames).Distinct().ToArray();
		}
	}
}
