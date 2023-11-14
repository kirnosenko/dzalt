using MediatR;

namespace DZALT.Entities.Tracing.TraceLogs
{
	public record TraceLogsCommand : IRequest
	{
		private TraceLogsCommand()
		{
		}

		public static TraceLogsCommand Create()
			=> new TraceLogsCommand();
	}
}
