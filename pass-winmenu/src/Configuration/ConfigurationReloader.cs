using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace PassWinmenu.Configuration
{
	public sealed class ConfigurationReloader : IDisposable
	{
		private readonly Action reloadAction;
		private FileSystemWatcher watcher;

		public ConfigurationReloader(Action reloadAction)
		{
			this.reloadAction = reloadAction;
		}

		public void EnableAutoReloading(string fileName)
		{
			var directory = Path.GetDirectoryName(Path.GetFullPath(fileName));
			if (string.IsNullOrEmpty(directory))
			{
				directory = Directory.GetCurrentDirectory();
			}

			watcher = new FileSystemWatcher(directory)
			{
				IncludeSubdirectories = false,
				EnableRaisingEvents = true
			};
			watcher.Changed += (sender, args) =>
			{
				if (args.Name == fileName)
				{
					// Wait a moment to allow the writing process to close the file.
					Thread.Sleep(500);
					Log.Send($"Configuration file changed (change type: {args.ChangeType}), attempting reload.");

					// This needs to be done on the main thread reloading the configuration file
					// involves creating UI resources (Brush/Thickness) that need to be created
					// on the same thread as the thread that will apply those resources to the interface.
					Application.Current.Dispatcher.Invoke(reloadAction);
				}
			};
		}

		public void Dispose()
		{
			watcher?.Dispose();
		}
	}
}
