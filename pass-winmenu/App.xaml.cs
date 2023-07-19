using System;
using System.CommandLine;
using System.Windows;
using PassWinmenu.Configuration;
using PassWinmenu.Utilities;
using PassWinmenu.WinApi;
using PassWinmenu.Windows;

using CommandLine=System.CommandLine;

namespace PassWinmenu
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public sealed partial class App : Application
	{
		private MainWindow? mainWindow;
		private IDisposable? program;
		private INotificationService? notificationService;
		
		private void App_Startup(object sender, StartupEventArgs e)
		{
			Current.Exit += HandleExit;
			var command = new RootCommand("View and manage passwords");

			var configFile = new CommandLine.Option<string>(
				"--config-file",
				"Path to the configuration file, relative to the main executable");
			configFile.SetDefaultValue("pass-winmenu.yaml");
			command.AddGlobalOption(configFile);
			
			command.SetHandler(HandleRun, configFile);

			command.Invoke(Environment.GetCommandLineArgs()[1..]);
		}

		private void HandleRun(string configPath)
		{
			var dialogService = new GraphicalDialogService();
			var configManager = ConfigurationLoader.Load(dialogService, configPath);
			notificationService = Notifications.Create();

			program = configManager.Match(
				c => Program.Start(notificationService, dialogService, c),
				() => Disposable.Empty);

			mainWindow = new MainWindow();
		}

		private void HandleExit(object sender, ExitEventArgs e)
		{
			mainWindow?.Close();
			program?.Dispose();
			notificationService?.Dispose();
			Environment.Exit(0);
		}

		public new static void Exit()
		{
			Current.Shutdown();
		}
	}
}
