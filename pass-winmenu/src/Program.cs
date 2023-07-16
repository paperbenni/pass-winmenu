using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using PassWinmenu.Actions;
using PassWinmenu.Configuration;
using PassWinmenu.ExternalPrograms;
using PassWinmenu.ExternalPrograms.Gpg;
using PassWinmenu.Hotkeys;
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

		public static IDisposable? Start(INotificationService notificationService, IDialogService dialogService, ConfigManager configManager)
		{
			IContainer? container = null;
			try
			{
				container = Initialise(notificationService, dialogService, configManager);
				Start(container, notificationService, dialogService);
				RunInitialCheck(container, dialogService);
			}
			catch (Exception e)
			{
				Log.EnableFileLogging();
				Log.Send("Could not start pass-winmenu: An exception occurred.", LogLevel.Error);
				Log.ReportException(e);

				if (e is DependencyResolutionException de && de.InnerException != null)
				{
					e = de.InnerException;
				}

				var errorMessage = $"pass-winmenu failed to start ({e.GetType().Name}: {e.Message})";
				dialogService.ShowErrorWindow(errorMessage);
				container?.Dispose();
				App.Exit();
			}

			return container;
		}

		/// <summary>
		/// Loads all required resources.
		/// </summary>
		private static IContainer Initialise(INotificationService notificationService, IDialogService dialogService, ConfigManager configManager)
		{
			// Load compiled-in resources.
			EmbeddedResources.Load();

			Log.Send("------------------------------");
			Log.Send($"Starting pass-winmenu {Version}");
			Log.Send("------------------------------");

			Log.Send($"Enabled security protocols: {ServicePointManager.SecurityProtocol}");

#if DEBUG
			Log.EnableFileLogging();
#else
			if (ConfigManager.Config.CreateLogFile)
			{
				Log.EnableFileLogging();
			}
#endif

			var container = new DependenciesBuilder()
				.RegisterNotifications(notificationService, dialogService)
				.RegisterConfiguration(configManager)
				.RegisterEnvironment()
				.RegisterActions()
				.RegisterGpg()
				.RegisterGit()
				.RegisterApplication()
				.Build();

			return container;
		}

		private static void Start(IContainer container, INotificationService notificationService, IDialogService dialogService)
		{
			var gpgConfig = container.Resolve<GpgConfig>();
			if (gpgConfig.GpgAgent.Config.AllowConfigManagement)
			{
				container.Resolve<GpgAgentConfigUpdater>().UpdateAgentConfig(gpgConfig.GpgAgent.Config.Keys);
			}

			var actionDispatcher = container.Resolve<ActionDispatcher>();
			var hotkeyService = container.Resolve<HotkeyService>();
			var hotkeys = container.Resolve<Config>().Hotkeys;
			
			// TODO: Not great, not terrible
			if (notificationService is INotifyIcon n)
			{
				n.AddMenuActions(actionDispatcher);
			}
			AssignHotkeys(hotkeys, actionDispatcher, hotkeyService, notificationService, dialogService);

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
		private static void RunInitialCheck(IContainer container, IDialogService dialogService)
		{
			var gpg = container.Resolve<GPG>();
			var passwordStoreConfig = container.Resolve<PasswordStoreConfig>();
			var gpgAgentConfig = container.Resolve<GpgAgentConfig>();

			if (!Directory.Exists(passwordStoreConfig.Location))
			{
				dialogService.ShowErrorWindow($"Could not find the password store at {Path.GetFullPath(passwordStoreConfig.Location)}. Please make sure it exists.");
				App.Exit();
				return;
			}
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
		private static void AssignHotkeys(
			IEnumerable<HotkeyConfig> hotkeys,
			ActionDispatcher actionDispatcher,
			HotkeyService hotkeyService,
			INotificationService notificationService,
			IDialogService dialogService)
		{
			try
			{
				hotkeyService.AssignHotkeys(
					hotkeys,
					actionDispatcher,
					notificationService!);
			}
			catch (Exception e) when (e is HotkeyException)
			{
				Log.Send("Failed to register hotkeys", LogLevel.Error);
				Log.ReportException(e);

				dialogService.ShowErrorWindow(e.Message, "Could not register hotkeys");
				App.Exit();
			}
		}
	}
}
