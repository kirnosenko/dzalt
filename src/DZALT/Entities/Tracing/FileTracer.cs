using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
		private static readonly Regex LogDateExp = new Regex(
			@"^AdminLog started on (?<date>.*?) at (?<time>.*?)$");

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
			using (var stream = OpenFile(filename))
			using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024))
			{
				string line = null;
				DateTime dateFrom = DateTime.MinValue;
				DateOnly date = DateOnly.MinValue;
				TimeOnly time = TimeOnly.MinValue;

				while ((line = reader.ReadLine()) != null)
				{
					if (date == DateOnly.MinValue)
					{
						var match = LogDateExp.Match(line);

						if (match.Success)
						{
							string dateMatch = match.Groups["date"].Value;
							string timeMatch = match.Groups["time"].Value;

							date = DateOnly.Parse(dateMatch);
							time = TimeOnly.Parse(timeMatch);
							dateFrom = date.ToDateTime(time);
						}
					}
					else
					{
						var log = await lineTracer.Trace(line, cancellationToken);
						var logTime = TimeOnly.FromDateTime(log.Date);

						if (logTime < time)
						{
							date = date.AddDays(1);
						}

						time = logTime;
						log.Date = date.ToDateTime(time);
						if (dateFrom == DateTime.MinValue)
						{
							dateFrom = log.Date;
						}
					}
				}

				session.Add(new LogFile()
				{
					Name = Path.GetFileNameWithoutExtension(filename),
					DateFrom = dateFrom,
					DateTo = date.ToDateTime(time),
				});

				await session.SubmitChanges(cancellationToken);
			}
		}

		protected virtual Stream OpenFile(string filename)
		{
			return File.OpenRead(filename);
		}
	}
}
