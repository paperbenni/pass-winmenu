using System;
using System.CommandLine;
using System.Windows;
using Autofac;
using PassWinmenu.Configuration;
using PassWinmenu.PasswordManagement;
using PassWinmenu.Utilities;
using PassWinmenu.WinApi;
using PassWinmenu.Windows;
using PInvoke;

using CommandLine=System.CommandLine;

namespace PassWinmenu
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public sealed partial class App : Application
	{
		private MainWindow? mainWindow;
		private INotificationService? notificationService;
		
		private bool remain;
		private bool hasConsole;
		
		private void App_Startup(object sender, StartupEventArgs e)
		{
			Current.Exit += HandleExit;
			hasConsole = Kernel32.AttachConsole(-1);
			var command = new RootCommand("View and manage passwords");

			var configFile = new CommandLine.Option<string>(
				"--config-file",
				"Path to the configuration file, relative to the main executable");
			configFile.SetDefaultValue("pass-winmenu.yaml");
			command.AddGlobalOption(configFile);

			var run = new Command("run", "Run pass-winmenu in GUI mode");
			run.SetHandler(HandleRun, configFile);
			var show = new Command("show", "Show a password");
			var all = new CommandLine.Option<bool>("--all", "Show the entire file, not just the password line");
			var path = new Argument<string>("path", "Path to the password to be shown");
			show.AddArgument(path);
			show.AddOption(all);
			show.SetHandler(HandleShow, configFile, path, all);

			command.Add(run);
			command.Add(show);
			command.SetHandler(HandleRun, configFile);

			var exitCode = command.Invoke(Environment.GetCommandLineArgs()[1..]);
			if (!remain)
			{
				Shutdown(exitCode);
			}
		}

		private void HandleRun(string configPath)
		{
			remain = true;

			var dialogService = new GraphicalDialogService();
			var configManager = ConfigurationLoader.Load(dialogService, configPath);
			notificationService = Notifications.Create();

			using var program = configManager.Match(
				c => Program.Start(notificationService, dialogService, c),
				() => Disposable.Empty);

			mainWindow = new MainWindow();
		}

		private void HandleShow(string configPath, string passwordPath, bool all)
		{
			var dialogService = new CommandLineDialogService();
			notificationService = new StubNotificationService();
			var configManager = ConfigurationLoader.Load(dialogService, configPath);
			
			var container = configManager.Select(c => Setup.Initialise(notificationService, dialogService, c));
			
			container.Apply(c =>
			 {
				 var manager = c.Resolve<IPasswordManager>();
				 if (!passwordPath.EndsWith(Program.EncryptedFileExtension))
				 {
					 passwordPath += Program.EncryptedFileExtension;
				 }
				 var password = manager.QueryPasswordFile(passwordPath);

				 Utilities.Option<string> decrypted;
				 if (all)
				 {
					decrypted = password.Select(p => manager.DecryptPassword(p, false).Content);
				 }
				 else
				 {
					decrypted = password.Select(p => manager.DecryptPassword(p, true).Password);
				 }
			  
				 var exitCode = decrypted.Match(pw =>
				 {
					  Console.WriteLine(pw);
					  return 0;
				 },
				 () => {
					  Console.WriteLine("Password does not exist!");
					  return 1;
				 });
			  
				 Current.Shutdown(exitCode);
			 });
		}

		private void HandleExit(object sender, ExitEventArgs e)
		{
			Kernel32.FreeConsole();
			mainWindow?.Close();
			notificationService?.Dispose();
			Environment.Exit(0);
		}

		public new static void Exit()
		{
			Current.Shutdown();
		}
	}
}
