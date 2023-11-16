using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Tracing
{
	public interface IDirTracer
	{
		Task Trace(string dir, CancellationToken cancellationToken);
	}

	public class DirTracer : IDirTracer
    {
		private readonly IRepository repository;
		private readonly IFileTracer fileTracer;

		public DirTracer(
			IRepository repository,
			IFileTracer fileTracer)
		{
			this.repository = repository;
			this.fileTracer = fileTracer;
		}

		public async Task Trace(string dir, CancellationToken cancellationToken)
		{
			var files = GetFiles(dir);

			if (files.Length > 0)
			{
				var filesOld = await repository.Get<LogFile>()
					.Select(x => x.Name)
					.ToArrayAsync(cancellationToken);
				var filesNew = files
					.Where(x => !filesOld.Contains(Path.GetFileNameWithoutExtension(x)))
					.ToArray();

				foreach (var file in filesNew)
				{
					await fileTracer.Trace(file, cancellationToken);
				}
			}
		}

		protected virtual string[] GetFiles(string dir)
		{
			if (Directory.Exists(dir))
			{
				return Directory.GetFiles(dir, "*.ADM");
			}

			return Array.Empty<string>();
		}
	}
}
