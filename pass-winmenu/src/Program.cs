using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using PassWinmenu.Actions;
using PassWinmenu.Configuration;
using PassWinmenu.ExternalPrograms;
using PassWinmenu.ExternalPrograms.Gpg;
using PassWinmenu.Hotkeys;
using PassWinmenu.Notifications;
using PassWinmenu.UpdateChecking;
using PassWinmenu.Utilities;
using PassWinmenu.WinApi;

#nullable enable
namespace PassWinmenu
{
	internal sealed class Program
	{
		public static string Version => EmbeddedResources.Version;

		public const string LastConfigVersion = "1.7";
		public const string EncryptedFileExtension = ".gpg";
		public const string PlaintextFileExtension = ".txt";

		public static IDisposable? Start(ConfigManager configManager)
		{
			IContainer? container = null;
			try
			{
				container = Setup.InitialiseDesktop(configManager);
				Start(container);
				RunInitialCheck(container);
			}
			catch (Exception e)
			{
				Log.EnableFileLogging();
				Log.Send("Could not start pass-winmenu: An exception occurred.", LogLevel.Error);
				Log.ReportException(e);

				if (e is DependencyResolutionException {InnerException: not null} de)
				{
					e = de.InnerException;
				}

				var errorMessage = $"pass-winmenu failed to start ({e.GetType().Name}: {e.Message})";
				new GraphicalDialogService().ShowErrorWindow(errorMessage);
				container?.Dispose();
				App.Exit();
			}

			return container;
		}

		private static void Start(IContainer container)
		{
			var gpgConfig = container.Resolve<GpgConfig>();
			if (gpgConfig.GpgAgent.Config.AllowConfigManagement)
			{
				container.Resolve<GpgAgentConfigUpdater>().UpdateAgentConfig(gpgConfig.GpgAgent.Config.Keys);
			}

			container.Resolve<INotifyIcon>().AddMenuActions(container.Resolve<ActionDispatcher>());
			
			AssignHotkeys(container);

			if (container.Resolve<UpdateCheckingConfig>().CheckForUpdates)
			{
				container.Resolve<UpdateChecker>().Start();
			}

			container.Resolve<Option<RemoteUpdateChecker>>().Apply(c => c.Start());

			var applicationConfig = container.Resolve<ApplicationConfig>();
			if (applicationConfig.ReloadConfig)
			{
				var configManager = container.Resolve<ConfigManager>();
				configManager.EnableAutoReloading();
			}
		}

		/// <summary>
		/// Checks if all components are configured correctly.
		/// </summary>
		private static void RunInitialCheck(IContainer container)
		{
			var gpg = container.Resolve<GPG>();
			var gpgAgentConfig = container.Resolve<GpgAgentConfig>();
			var dialogService = container.Resolve<IDialogService>();

			try
			{
				Log.Send("Using GPG version " + gpg.GetVersion());
			}
			catch (System.ComponentModel.Win32Exception)
			{
				dialogService.ShowErrorWindow("Could not find GPG. Make sure your gpg-path is set correctly.");
				App.Exit();
				return;
			}
			catch (Exception e)
			{
				dialogService.ShowErrorWindow($"Failed to initialise GPG. {e.GetType().Name}: {e.Message}");
				App.Exit();
				return;
			}
			if (gpgAgentConfig.Preload)
			{
				Task.Run(() =>
				{
					try
					{
						gpg.StartAgent();
					}
					catch (GpgError err)
					{
						dialogService.ShowErrorWindow(err.Message);
					}
					// Ignore other exceptions. If it turns out GPG is misconfigured,
					// these errors will surface upon decryption/encryption.
					// The reason we catch GpgErrors here is so we can notify the user
					// if we don't detect any decryption keys.
				});
			}
		}


		/// <summary>
		/// Loads keybindings from the configuration file and registers them with Windows.
		/// </summary>
		private static void AssignHotkeys(IContainer container)
		{
			var hotkeyService = container.Resolve<HotkeyService>();
			var hotkeys = container.Resolve<Config>().Hotkeys;
			var actionDispatcher = container.Resolve<ActionDispatcher>();
			
			try
			{
				hotkeyService.AssignHotkeys(hotkeys, actionDispatcher);
			}
			catch (Exception e) when (e is HotkeyException)
			{
				Log.Send("Failed to register hotkeys", LogLevel.Error);
				Log.ReportException(e);

				container.Resolve<IDialogService>().ShowErrorWindow(e.Message, "Could not register hotkeys");
				App.Exit();
			}
		}
	}
}
