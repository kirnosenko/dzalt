using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DZALT.Entities.Tracing
{
	public class DirTracerTest : BaseRepositoryTest
	{
		public static string[] files = new string[]
		{
			"c:\\file1.ADM",
			"c:\\file2.ADM",
			"c:\\file3.ADM",
		};

		private class TestableDirTracer : DirTracer
		{
			public TestableDirTracer(
				IRepository repository,
				IFileTracer fileTracer)
				: base(repository, fileTracer)
			{
			}

			protected override string[] GetFiles(string dir)
			{
				return files;
			}
		}

		private readonly IFileTracer fileTracer;
		private readonly IDirTracer dirTracer;
		
		public DirTracerTest()
		{
			fileTracer = Substitute.For<IFileTracer>();
			dirTracer = new TestableDirTracer(this, fileTracer);
		}

		[Fact]
		public async Task ShouldSaveTracedFileNames()
		{
			List<string> calls = new List<string>();
			fileTracer.Trace(
				Arg.Any<string>(),
				Arg.Any<CancellationToken>())
				.Returns(ci =>
				{
					calls.Add(ci[0] as string);
					return Task.CompletedTask;
				});

			await dirTracer.Trace(null, default);

			calls.Count.Should().Be(files.Length);
			calls.Should().BeEquivalentTo(files);
		}

		[Fact]
		public async Task ShouldProcessUnprocessedFilesOnly()
		{
			for (int i = 0; i < files.Length - 1; i++)
			{
				Add(new LogFile()
				{
					Name = Path.GetFileNameWithoutExtension(files[i]),
				});
			}
			await SubmitChanges();

			List<string> calls = new List<string>();
			fileTracer.Trace(
				Arg.Any<string>(),
				Arg.Any<CancellationToken>())
				.Returns(ci =>
				{
					calls.Add(ci[0] as string);
					return Task.CompletedTask;
				});

			await dirTracer.Trace(null, default);

			calls.Count.Should().Be(1);
			calls.Should().BeEquivalentTo(new string[]
			{
				files.Last(),
			});
		}
	}
}
