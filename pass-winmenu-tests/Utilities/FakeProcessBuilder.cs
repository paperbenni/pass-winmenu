using System;
using System.IO;
using System.Text;

namespace PassWinmenuTests.Utilities
{
	public class FakeProcessBuilder
	{
		private TimeSpan exitTime;
		private StreamReader standardError;
		private StreamReader standardOutput;
		private int exitCode;
		private Stream inputStream;

		public FakeProcess Build()
		{
			return new FakeProcess
			{
				ExitCode = exitCode,
				ExitTime = exitTime,
				StandardError = standardError,
				StandardOutput = standardOutput,
				StandardInput = inputStream == null ? null : new StreamWriter(inputStream)
			};
		}

		public FakeProcessBuilder WithStandardError(string stderr)
		{
			standardError = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(stderr)));
			return this;
		}

		public FakeProcessBuilder WithExitTime(TimeSpan timeSpan)
		{
			exitTime = timeSpan;
			return this;
		}

		public FakeProcessBuilder WithStandardOutput(string stdout)
		{
			standardOutput = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(stdout)));
			return this;
		}

		public FakeProcessBuilder WithExitCode(int code)
		{
			exitCode = code;
			return this;
		}

		public FakeProcessBuilder WithStandardInput(Stream stream)
		{
			inputStream = stream;
			return this;
		}
	}
}
