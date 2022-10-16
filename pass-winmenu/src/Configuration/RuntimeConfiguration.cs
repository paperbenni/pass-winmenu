using System.IO;
using System.Reflection;

#nullable enable
namespace PassWinmenu.Configuration
{
	internal class RuntimeConfiguration
	{
		public string ConfigFileLocation { get; private set; } = "pass-winmenu.yaml";

		private RuntimeConfiguration()
		{
		}

		internal static RuntimeConfiguration Parse(string[] args)
		{
			var executableDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
			var defaultConfigPath = Path.Combine(executableDirectory!, "pass-winmenu.yaml");

			var configuration = new RuntimeConfiguration
			{
				ConfigFileLocation = defaultConfigPath,
			};

			if (args.Length > 1)
			{
				if (args.Length == 3 && args[1] == "--config-file")
				{
					configuration.ConfigFileLocation = args[2];
				}
				else
				{
					throw new RuntimeConfigurationException($"Invalid argument: {args[1]}");
				}
			}


			return configuration;
		}

	}
}
