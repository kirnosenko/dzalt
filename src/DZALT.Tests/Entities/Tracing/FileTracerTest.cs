using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DZALT.Entities.Tracing
{
	public class FileTracerTest : BaseRepositoryTest
	{
		private class TestableFileTracer : FileTracer
		{
			public TestableFileTracer(
				ISession session,
				ILineTracer lineTracer)
				: base(session, lineTracer)
			{
			}

			public string[] FileLines { get; set; }

			protected override Stream OpenFile(string filename)
			{
				var buffer = new MemoryStream();
				using (TextWriter writer = new StreamWriter(buffer, leaveOpen: true))
				{
					foreach (var line in FileLines)
					{
						writer.WriteLine(line);
					}
					writer.Flush();
				}
				buffer.Seek(0, SeekOrigin.Begin);

				return buffer;
			}
		}

		private readonly ILineTracer lineTracer;
		private readonly TestableFileTracer fileTracer;

		public FileTracerTest()
		{
			lineTracer = Substitute.For<ILineTracer>();
			fileTracer = new TestableFileTracer(this, lineTracer);
		}

		[Fact]
		public async Task ShouldSetDateForLog()
		{
			var log1 = new SessionLog()
			{
				Date = new DateTime(1, 1, 1, 13, 10, 20),
			};
			var log2 = new SessionLog()
			{
				Date = new DateTime(1, 1, 1, 14, 10, 20),
			};
			fileTracer.FileLines = new string[]
			{
				"AdminLog started on 2023-11-16 at 12:23:45",
				"line1",
				"line2",
			};
			lineTracer.Trace(
				Arg.Any<string>(),
				Arg.Any<CancellationToken>())
				.Returns(ci => (ci[0] as string) == "line1" ? log1 : log2);

			await fileTracer.Trace("filename", default);

			await lineTracer.Received(2).Trace(
				Arg.Any<string>(),
				Arg.Any<CancellationToken>());
			log1.Date
				.Should().Be(new DateTime(2023, 11, 16, 13, 10, 20));
			log2.Date
				.Should().Be(new DateTime(2023, 11, 16, 14, 10, 20));
		}

		[Fact]
		public async Task ShouldSetDateForLogAfterMidnight()
		{
			var log1 = new SessionLog()
			{
				Date = new DateTime(1, 1, 1, 23, 50, 00),
			};
			var log2 = new SessionLog()
			{
				Date = new DateTime(1, 1, 1, 00, 10, 00),
			};
			fileTracer.FileLines = new string[]
			{
				"AdminLog started on 2023-11-16 at 12:23:45",
				"line1",
				"line2",
			};
			lineTracer.Trace(
				Arg.Any<string>(),
				Arg.Any<CancellationToken>())
				.Returns(ci => (ci[0] as string) == "line1" ? log1 : log2);

			await fileTracer.Trace("filename", default);

			await lineTracer.Received(2).Trace(
				Arg.Any<string>(),
				Arg.Any<CancellationToken>());
			log1.Date
				.Should().Be(new DateTime(2023, 11, 16, 23, 50, 00));
			log2.Date
				.Should().Be(new DateTime(2023, 11, 17, 00, 10, 00));
		}

		[Fact]
		public async Task ShouldIgnoreNullLog()
		{
			fileTracer.FileLines = new string[]
			{
				"AdminLog started on 2023-11-16 at 12:23:45",
				"line1",
				"line2",
				"line3",
			};
			lineTracer.Trace(
				Arg.Any<string>(),
				Arg.Any<CancellationToken>())
				.Returns(ci => (ci[0] as string) == "line2" ? null : new SessionLog());

			await fileTracer.Trace("filename", default);

			await lineTracer.Received(3).Trace(
				Arg.Any<string>(),
				Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task ShouldSaveLogFileDataWithProcessedData()
		{
			var log1 = new SessionLog()
			{
				Date = new DateTime(1, 1, 1, 13, 10, 20),
			};
			var log2 = new SessionLog()
			{
				Date = new DateTime(1, 1, 1, 14, 10, 20),
			};
			fileTracer.FileLines = new string[]
			{
				"AdminLog started on 2023-11-16 at 12:23:45",
				"line1",
				"line2",
			};
			lineTracer.Trace(
				Arg.Any<string>(),
				Arg.Any<CancellationToken>())
				.Returns(ci => (ci[0] as string) == "line1" ? log1 : log2);

			await fileTracer.Trace("c:\\filename.ADM", default);

			var logFile = Get<LogFile>().SingleOrDefault();
			logFile.Should().NotBeNull();
			logFile.Name.Should().Be("filename");
			logFile.DateFrom.Should().Be(new DateTime(2023, 11, 16, 12, 23, 45));
			logFile.DateTo.Should().Be(new DateTime(2023, 11, 16, 14, 10, 20));
		}

		[Fact]
		public async Task ShouldSaveLogFileDataWhenNoData()
		{
			fileTracer.FileLines = new string[]
			{
				"AdminLog started on 2023-11-16 at 12:23:45",
				
			};
			lineTracer.Trace(
				Arg.Any<string>(),
				Arg.Any<CancellationToken>())
				.Returns((Log)null);

			await fileTracer.Trace("c:\\filename.ADM", default);

			var logFile = Get<LogFile>().SingleOrDefault();
			logFile.Should().NotBeNull();
			logFile.Name.Should().Be("filename");
			logFile.DateFrom.Should().Be(new DateTime(2023, 11, 16, 12, 23, 45));
			logFile.DateTo.Should().Be(new DateTime(2023, 11, 16, 12, 23, 45));
		}
	}
}
