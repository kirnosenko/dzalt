using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace DZALT.Entities.Tracing.TraceLogs
{
	public class TraceLogsHandler : IRequestHandler<TraceLogsCommand>
	{
		private readonly IDirTracer dirTracer;

		public TraceLogsHandler(IDirTracer dirTracer)
		{
			this.dirTracer = dirTracer;
		}

		public async Task Handle(
			TraceLogsCommand command,
			CancellationToken cancellationToken)
		{
			var logsDir = Environment.GetEnvironmentVariable(EnvironmentVariables.DZALT_LOGS_PATH);

			await dirTracer.Trace(logsDir, cancellationToken);
		}
	}
}
