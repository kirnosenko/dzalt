using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Selection.MultipleHits
{
	public class MultipleHitsHandler : IRequestHandler<MultipleHitsQuery, MultipleHitsResult[]>
	{
		private static Dictionary<string, int> hitsPerSecondLimit = new Dictionary<string, int>()
		{
			{ "AUR A1", 10 },
			{ "AUR AX", 10 },
			{ "Blaze", 2 },
			{ "CR-527", 1 },
			{ "CR-550 Savanna", 1 },
			{ "CR-61 Skorpion", 15 },
			{ "CR-75", 7 },
			{ "Deagle", 5 },
			{ "DMR", 7 },
			{ "FX-45", 7 },
			{ "Kolt 1911", 7 },
			{ "Kolt 1911 с рисунком", 7 },
			{ "LAR", 10 },
			{ "LE-MAS", 15 },
			{ "Обрез LE-MAS", 15 },
			{ "Longhorn", 1 },
			{ "M16-A2", 9 },
			{ "M4-A1", 15 },
			{ "M70 Tundra", 1 },
			{ "M79", 1 },
			{ "MK II", 7 },
			{ "Mlock-91", 7 },
			{ "P1", 7 },
			{ "Pioneer", 1 },
			{ "SG5-K", 15 },
			{ "Sporter 22", 7 },
			{ "SSG 82", 1 },
			{ "USG-45", 10 },
			{ "Арбалет", 1 },
			{ "БK-12", 1 }, // depends on ammo
			{ "Обрез БK-12", 1 }, // depends on ammo
			{ "БK-18", 8 },
			{ "Обрез БK-18", 8 },
			{ "Бизон", 10 },
			{ "БК-133", 1 }, // depends on ammo
			{ "БК-43", 1 },  // depends on ammo
			{ "Обрез БК-43", 1 }, // depends on ammo
			{ "Вайга", 7 }, // depends on ammo
			{ "Винтовка Мосин 91/30", 1 },
			{ "ВСД", 7 },
			{ "ВСС", 15 },
			{ "Дерринджер", 1 },
			{ "ИЖ-70", 7 },
			{ "КА-101", 10 },
			{ "КА-74", 10 },
			{ "КА-M", 10 },
			{ "Карабин c магазином", 1 },
			{ "КАС-74У", 10 },
			{ "Револьвер", 4 },
			{ "СВАЛ", 15 },
			{ "Сигнальный пистолет", 1 },
			{ "СК 59/66", 7 },
		};

		private readonly IRepository repository;

		public MultipleHitsHandler(IRepository repository)
		{
			this.repository = repository;
		}

		public async Task<MultipleHitsResult[]> Handle(
			MultipleHitsQuery query,
			CancellationToken cancellationToken)
		{
			var playerId = string.IsNullOrEmpty(query.PlayerNickOrGuid)
				? (int?)null
				: await repository.PlayerIdByName(query.PlayerNickOrGuid, cancellationToken);

			var hits = await (
				from e in repository.Get<EventLog>()
				where
					(query.From == null || query.From <= e.Date) &&
					(query.To == null || query.To >= e.Date) &&
					e.Event == EventLog.EventType.HIT &&
					e.EnemyPlayerId != null &&
					e.Distance != null &&
					(playerId == null || e.EnemyPlayerId == playerId)
				group e.Distance by new { e.EnemyPlayerId, e.Date, e.Weapon } into g
				select new MultipleHitsResult()
				{
					PlayerId = g.Key.EnemyPlayerId.Value,
					Date = g.Key.Date,
					Hits = g.Count(),
					Weapon = g.Key.Weapon,
					Distance = g.Average(x => x.Value),
				}).Where(x => x.Hits > 1).ToArrayAsync(cancellationToken);

			if (query.InvalidOnly)
			{
				hits = hits
					.Where(x => hitsPerSecondLimit.TryGetValue(x.Weapon, out var limit) ? x.Hits > limit : true)
					.ToArray();
			}

			return hits;
		}
	}
}
