using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Selection.PlayersWithMultipleNames
{
	public class PlayersWithMultipleNamesHandler : IRequestHandler<PlayersWithMultipleNamesQuery, PlayersWithMultipleNamesResult[]>
	{
		private readonly IRepository repository;

		public PlayersWithMultipleNamesHandler(IRepository repository)
		{
			this.repository = repository;
		}

		public async Task<PlayersWithMultipleNamesResult[]> Handle(
			PlayersWithMultipleNamesQuery query,
			CancellationToken cancellationToken)
		{
			var playerNames = await (
				from p in repository.Get<Player>()
				from s in repository.Get<SessionLog>()
					.Where(s => s.PlayerId == p.Id &&
								(query.From == null || query.From <= s.Date) &&
								(query.To == null || query.To >= s.Date))
					.Take(1).DefaultIfEmpty()
				join name in repository.Get<Nickname>()
					on p.Id equals name.PlayerId
				where s != null
				group name.Name by p.Id into g
				where g.Count() > 1
				select new PlayersWithMultipleNamesResult()
				{
					PlayerId = g.Key,
					Names = g.ToArray(),
				}).ToArrayAsync(cancellationToken);

			return playerNames;
		}
	}
}
