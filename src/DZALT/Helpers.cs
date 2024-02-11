using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DZALT.Entities;
using System.Collections.Generic;

namespace DZALT
{
	public static class Helpers
	{
		public static string FormatPlayerName(string guid, string nickname = null)
		{
			guid = guid.Length == 44 ? guid.Substring(44 - 1 - 8) : guid;
			nickname = nickname != null
				? $"{nickname} "
				: "";

			return $"{nickname}({guid})";
		}

		public static async Task<int> PlayerIdByName(
			this IRepository repository,
			string name,
			CancellationToken cancellationToken)
		{
			var playerIds = await(
				from p in repository.Get<Player>()
				join n in repository.Get<Nickname>()
					on p.Id equals n.PlayerId into leftjoin
				from x in leftjoin.DefaultIfEmpty()
				where
					x.Name == name ||
					p.Guid == name ||
					p.Guid.EndsWith(name)
				select p.Id).ToArrayAsync(cancellationToken);

			return playerIds.Length switch
			{
				0 => throw new InvalidOperationException($"No any players for name {name}."),
				> 1 => throw new InvalidOperationException($"There are several players for name {name}."),
				_ => playerIds[0],
			};
		}

		public static async Task<IDictionary<int, string>> PlayersNames(
			this IRepository repository,
			CancellationToken cancellationToken)
		{
			var playerNicknames = await (
				from p in repository.Get<Player>()
				let name = repository.Get<Nickname>()
					.Where(x => x.PlayerId == p.Id)
					.OrderByDescending(x => x.Id)
					.Select(x => x.Name)
					.FirstOrDefault()
				select new
				{
					Id = p.Id,
					Guid = p.Guid,
					Name = name,
				}).ToDictionaryAsync(x => x.Id, x => Helpers.FormatPlayerName(x.Guid, x.Name), cancellationToken);

			return playerNicknames;
		}
	}
}
