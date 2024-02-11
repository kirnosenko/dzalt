﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using DZALT.Entities.Selection.KillsByPlayer;
using DZALT.Entities.Selection.KillsPerHourByPlayer;
using DZALT.Entities.Selection.PlayerLog;
using DZALT.Entities.Selection.PlayerNames;
using DZALT.Entities.Selection.PlayerRawEvents;
using DZALT.Entities.Selection.PlayerShots;
using DZALT.Entities.Selection.PlayTimeByPlayer;
using DZALT.Entities.Selection.PlayTimeIntersection;
using DZALT.Entities.Selection.TimeTillFirstKill;
using DZALT.Entities.Selection.TouchedPlayers;
using System.Linq;

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
		public async Task<IActionResult> PlayerLog(
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
	}
}
