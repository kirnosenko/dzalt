using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DZALT.Entities.Tracing
{
	public interface IDirTracer
	{
		Task Trace(string dir, CancellationToken cancellationToken);
	}

	public class DirTracer : IDirTracer
    {
		private readonly IFileTracer fileTracer;

		public DirTracer(IFileTracer fileTracer)
		{
			this.fileTracer = fileTracer;
		}

		public async Task Trace(string dir, CancellationToken cancellationToken)
		{
			if (Directory.Exists(dir))
			{
				var files = Directory.GetFiles(dir, "*.ADM");
				foreach (var file in files)
				{
					await fileTracer.Trace(file, cancellationToken);
				}
			}
		}
	}
}
