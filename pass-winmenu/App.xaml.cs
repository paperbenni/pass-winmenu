using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Windows;
using PassWinmenu.WinApi;
using PassWinmenu.Windows;
using PInvoke;

#nullable enable
namespace PassWinmenu
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public sealed partial class App : Application
	{
		private static MainWindow? mainWindow;
		private bool remain;

		private void App_Startup(object sender, StartupEventArgs e)
		{
			Current.Exit += HandleExit;
			Kernel32.AttachConsole(-1);

			var command = new RootCommand("View and manage passwords");

			var configFile = new Option<string>(
				"--config-file",
				"Path to the configuration file, relative to the main executable");
			configFile.SetDefaultValue("pass-winmenu.yaml");
			command.AddGlobalOption(configFile);

			var run = new Command("run", "Run pass-winmenu in GUI mode");
			run.SetHandler(HandleRun, configFile);
			var show = new Command("show", "Show a password");
			show.SetHandler(HandleShow);
			show.AddArgument(new Argument<string>("path", "Path to the password to be shown"));

			command.Add(run);
			command.Add(show);
			command.SetHandler(HandleRun, configFile);
			
			var exitCode = command.Invoke(Environment.GetCommandLineArgs()[1..]);
			if (!remain)
			{
				Shutdown(exitCode);
			}
		}

		private void HandleRun(string configFile)
		{
			remain = true;
			var notifications = Notifications.Create();
			using var program = Program.Start(notifications, configFile);
			mainWindow = new MainWindow();
		}

		private void HandleShow(InvocationContext obj)
		{
			throw new NotImplementedException();
		}

		private void HandleExit(object sender, ExitEventArgs e)
		{
			Kernel32.FreeConsole();
			mainWindow?.Close();
			Environment.Exit(0);
		}

		public new static void Exit()
		{
			Current.Shutdown();
		}
	}
}
