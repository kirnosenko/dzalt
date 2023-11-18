using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MediatR;
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
		public async Task<IActionResult> PlayTimeByPlayerHandler(
			CancellationToken cancellationToken)
		{
			var data = await mediator.Send(PlayTimeByPlayerQuery.Instance, cancellationToken);

			return Ok(data);
		}
	}
}
