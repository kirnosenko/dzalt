using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Selection.SameNames
{
	public class SameNamesHandler : IRequestHandler<SameNamesQuery, SameNamesResult[]>
	{
		private readonly IRepository repository;

		public SameNamesHandler(IRepository repository)
		{
			this.repository = repository;
		}

		public async Task<SameNamesResult[]> Handle(
			SameNamesQuery query,
			CancellationToken cancellationToken)
		{
			var playerNames = await (
				from p in repository.Get<Player>()
				join name in repository.Get<Nickname>()
					on p.Id equals name.PlayerId
				group p.Guid by name.Name into g
				where g.Count() > 1
				select new SameNamesResult()
				{
					Name = g.Key,
					Guids = g.ToArray(),
				}).ToArrayAsync(cancellationToken);

			return playerNames;
		}
	}
}
