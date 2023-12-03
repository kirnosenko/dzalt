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
			var player = await (
				from p in repository.Get<Player>()
				join n in repository.Get<Nickname>()
					on p.Id equals n.PlayerId into leftjoin
				from x in leftjoin.DefaultIfEmpty()
				where p.Guid == playerName || x.Name == playerName
				select p).FirstOrDefaultAsync(cancellationToken);

			var nicknames = await repository.Get<Nickname>()
				.Where(x => x.PlayerId == player.Id)
				.Select(x => x.Name)
				.ToArrayAsync(cancellationToken);

			return new string[] { player.Guid }.Concat(nicknames).ToArray();
		}
	}
}
