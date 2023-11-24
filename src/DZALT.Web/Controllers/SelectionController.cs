﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using DZALT.Entities.Selection.KillsByPlayer;
using DZALT.Entities.Selection.NamesByPlayer;
using DZALT.Entities.Selection.PlayTimeByPlayer;
using DZALT.Entities.Selection.PlayerLog;
using DZALT.Entities.Selection.PlayTimeIntersection;

namespace DZALT.Web.Controllers
{
	[ApiController]
	[Produces("application/json")]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class SelectionController : ControllerBase
	{
		private readonly IMediator mediator;

		public SelectionController(IMediator mediator)
		{
			this.mediator = mediator;
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<IActionResult> NamesByPlayer(
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(
				NamesByPlayerQuery.Instance,
				cancellationToken);

			return Ok(data);
		}

		[HttpGet]
		[Route("[action]")]
		public async Task<IActionResult> PlayerLog(
			[FromQuery] string nickname,
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(
				PlayerLogQuery.Create(nickname),
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

			return Ok(data);
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

			return Ok(data);
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
	}
}
