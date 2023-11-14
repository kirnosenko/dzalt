using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using DZALT.Entities.Tracing;
using DZALT.Entities.Tracing.TraceLogs;

namespace DZALT.Web.Controllers
{
	[ApiController]
	[Produces("application/json")]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class TraceController : ControllerBase
	{
		private readonly IMediator mediator;

		public TraceController(IMediator mediator)
		{
			this.mediator = mediator;
		}

		[HttpPost]
		public async Task<IActionResult> Trace()
		{
			await mediator.Send(TraceLogsCommand.Create());

			return Ok();
		}
	}
}
