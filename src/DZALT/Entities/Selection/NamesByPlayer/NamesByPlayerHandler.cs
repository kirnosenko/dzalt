using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Selection.NamesByPlayer
{
	public class NamesByPlayerHandler : IRequestHandler<NamesByPlayerQuery, Dictionary<int, string>>
	{
		private readonly IRepository repository;

		public NamesByPlayerHandler(IRepository repository)
		{
			this.repository = repository;
		}

		public async Task<Dictionary<int, string>> Handle(
			NamesByPlayerQuery query,
			CancellationToken cancellationToken)
		{
			var playerNicknames = await (
				from p in repository.Get<Player>()
				let name = repository.Get<Nickname>()
					.Where(x => x.PlayerId == p.Id)
					.Select(x => x.Name)
					.FirstOrDefault()
				select new
				{
					Id = p.Id,
					Guid = p.Guid,
					Name = name,
				}).ToDictionaryAsync(x => x.Id, x => x.Name ?? x.Guid, cancellationToken);

			return playerNicknames;
		}
	}
}
