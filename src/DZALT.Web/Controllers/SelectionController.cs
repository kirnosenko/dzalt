using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using DZALT.Entities.Selection.KillsByPlayer;
using DZALT.Entities.Selection.KillsPerHourByPlayer;
using DZALT.Entities.Selection.MultipleHits;
using DZALT.Entities.Selection.PlayerLog;
using DZALT.Entities.Selection.PlayerNames;
using DZALT.Entities.Selection.PlayerRawEvents;
using DZALT.Entities.Selection.PlayerShots;
using DZALT.Entities.Selection.PlayersWithMultipleNames;
using DZALT.Entities.Selection.PlayTimeByPlayer;
using DZALT.Entities.Selection.PlayTimeIntersection;
using DZALT.Entities.Selection.SameNames;
using DZALT.Entities.Selection.TimeTillFirstKill;
using DZALT.Entities.Selection.TouchedPlayers;
using DZALT.Entities.Selection.Weapons;

namespace DZALT.Web.Controllers
{
	[ApiController]
	[Produces("application/json")]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class SelectionController : ControllerBase
	{
		private readonly IMediator mediator;
		private readonly IRepository repository;

		public SelectionController(
			IMediator mediator,
			IRepository repository)
		{
			this.mediator = mediator;
			this.repository = repository;
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<IActionResult> KillsByPlayer(
			[FromQuery] DateTime? from,
			[FromQuery] DateTime? to,
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(
				KillsByPlayerQuery.Create(from, to),
				cancellationToken);
			var names = await repository.PlayersNames(cancellationToken);

			return Ok(data
				.OrderByDescending(x => x.Kills)
				.Select(x => new
				{
					Name = names[x.PlayerId],
					x.Kills,
					x.Deaths,
				}));
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<IActionResult> KillsPerHourByPlayer(
			[FromQuery] DateTime? from,
			[FromQuery] DateTime? to,
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(
				KillsPerHourByPlayerQuery.Create(from, to),
				cancellationToken);
			var names = await repository.PlayersNames(cancellationToken);

			return Ok(data
				.OrderByDescending(x => x.KillsPerHour)
				.Select(x => new
				{
					Name = names[x.PlayerId],
					x.KillsPerHour,
					x.Kills,
					x.Time,
				}));
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<IActionResult> MultipleHits(
			[FromQuery] DateTime? from,
			[FromQuery] DateTime? to,
			[FromQuery] string playerNickOrGuid,
			[FromQuery] bool invalidOnly,
			[FromQuery] bool perPlayer,
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(
				MultipleHitsQuery.Create(from, to, playerNickOrGuid, invalidOnly),
				cancellationToken);
			var names = await repository.PlayersNames(cancellationToken);

			return Ok(!perPlayer
				? data
					.OrderBy(x => x.Weapon)
					.ThenByDescending(x => x.Hits)
					.Select(x => new
					{
						Name = names[x.PlayerId],
						x.Date,
						x.Hits,
						x.Weapon,
						x.Distance,
					})
				: data
					.GroupBy(x => x.PlayerId)
					.OrderByDescending(x => x.Count())
					.Select(x => new
					{
						Name = names[x.Key],
						Count = x.Count(),
					}));
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<IActionResult> PlayerLog(
			[FromQuery] DateTime? from,
			[FromQuery] DateTime? to,
			[FromQuery] string nickname,
			[FromQuery] bool includeSessions,
			[FromQuery] bool includeHits,
			[FromQuery] bool includeKills,
			[FromQuery] bool includeSuicides,
			[FromQuery] bool includeAccidents,
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(
				PlayerLogQuery.Create(
					from,
					to,
					nickname,
					includeSessions,
					includeHits,
					includeKills,
					includeSuicides,
					includeAccidents),
				cancellationToken);

			return Ok(data.Select(x => x.ToString()));
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<IActionResult> PlayerLongestShots(
			[FromQuery] string[] bodyparts,
			[FromQuery] DateTime? from,
			[FromQuery] DateTime? to,
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(
				PlayerShotsQuery.Create(bodyparts, from, to),
				cancellationToken);
			var names = await repository.PlayersNames(cancellationToken);

			return Ok(data
				.OrderByDescending(x => x.Distance)
				.Select(x => new
				{
					x.Date,
					Attacker = names[x.AttackerId],
					Victim = names[x.VictimId],
					x.Weapon,
					x.Bodypart,
					x.Distance,
				}));
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<IActionResult> PlayerNames(
			[FromQuery] string nickname,
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(
				PlayerNamesQuery.Create(nickname),
				cancellationToken);

			return Ok(data);
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<IActionResult> PlayerRawEvents(
			[FromQuery] string nickname,
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(
				PlayerRawEventsQuery.Create(nickname),
				cancellationToken);

			return Ok(data);
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<IActionResult> PlayersWithMultipleNames(
			[FromQuery] DateTime? from,
			[FromQuery] DateTime? to,
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(
				PlayersWithMultipleNamesQuery.Create(from, to),
				cancellationToken);
			
			return Ok(data);
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<IActionResult> PlayTimeByPlayer(
			[FromQuery] DateTime? from,
			[FromQuery] DateTime? to,
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(
				PlayTimeByPlayerQuery.Create(from, to),
				cancellationToken);
			var names = await repository.PlayersNames(cancellationToken);

			return Ok(data
				.OrderByDescending(x => x.Time)
				.Select(x => new
				{
					Name = names[x.PlayerId],
					x.Time,
				}));
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<IActionResult> PlayTimeIntersection(
			[FromQuery] string player1,
			[FromQuery] string player2,
			[FromQuery] DateTime? from,
			[FromQuery] DateTime? to,
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(
				PlayTimeIntersectionQuery.Create(player1, player2, from, to),
				cancellationToken);

			return Ok(data);
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<IActionResult> SameNamesHandler(
			[FromQuery] DateTime? from,
			[FromQuery] DateTime? to,
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(
				SameNamesQuery.Create(from, to),
				cancellationToken);

			return Ok(data);
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<IActionResult> TimeTillFirstKill(
			[FromQuery] DateTime? from,
			[FromQuery] DateTime? to,
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(
				TimeTillFirstKillQuery.Create(from, to),
				cancellationToken);
			var names = await repository.PlayersNames(cancellationToken);

			return Ok(data
				.OrderByDescending(x => x.Time)
				.Select(x => new
				{
					Name = names[x.PlayerId],
					x.DeathDate,
					x.KillDate,
					x.Time,
				}));
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<IActionResult> TouchedPlayers(
			[FromQuery] DateTime? from,
			[FromQuery] DateTime? to,
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(
				TouchedPlayersQuery.Create(from, to),
				cancellationToken);

			return Ok(data);
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<IActionResult> Weapons(
			[FromQuery] DateTime? from,
			[FromQuery] DateTime? to,
			[FromQuery] bool excludeMelee,
			[FromQuery] bool excludeDistance,
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(
				WeaponsQuery.Create(from, to, excludeMelee, excludeDistance),
				cancellationToken);
			
			return Ok(data.OrderBy(x => x.Weapon));
		}
	}
}
