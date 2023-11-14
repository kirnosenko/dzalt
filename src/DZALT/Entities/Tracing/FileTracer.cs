using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DZALT.Entities.Tracing
{
	public interface IFileTracer
	{
		Task Trace(string filename, CancellationToken cancellationToken);
	}

	public class FileTracer : IFileTracer
    {
		private readonly ISession session;
		private readonly ILineTracer lineTracer;

		public FileTracer(
			ISession session,
			ILineTracer lineTracer)
		{
			this.session = session;
			this.lineTracer = lineTracer;
		}

		public async Task Trace(string filename, CancellationToken cancellationToken)
		{
			using (var stream = File.OpenRead(filename))
			using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024))
			{
				string line = null;

				while ((line = reader.ReadLine()) != null)
				{
					var log = await lineTracer.Trace(line, cancellationToken);
				}
			}

			await session.SubmitChanges(cancellationToken);
		}
	}
}
