using System;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using DZALT.Entities.Selection.KillsByPlayer;
using DZALT.Entities.Selection.PlayTimeByPlayer;

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
	}
}
