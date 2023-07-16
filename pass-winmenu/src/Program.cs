using System;
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

		public static IDisposable? Start(INotificationService notifications)
		{
			IContainer? container = null;
			try
			{
				container = Initialise(notifications);
				Start(container, notifications);
				RunInitialCheck(container, notifications);
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
				notifications.ShowErrorWindow(errorMessage);
				notifications.Dispose();
				container?.Dispose();
				App.Exit();
			}

			return container;
		}

		/// <summary>
		/// Loads all required resources.
		/// </summary>
		private static IContainer Initialise(INotificationService notifications)
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

			var container = new DependenciesBuilder(notifications)
				.RegisterNotifications()
				.RegisterConfiguration()
				.RegisterEnvironment()
				.RegisterActions()
				.RegisterGpg()
				.RegisterGit()
				.RegisterApplication()
				.Build();

			return container;
		}

		private static void Start(IContainer container, INotificationService notifications)
		{
			var gpgConfig = container.Resolve<GpgConfig>();
			if (gpgConfig.GpgAgent.Config.AllowConfigManagement)
			{
				container.Resolve<GpgAgentConfigUpdater>().UpdateAgentConfig(gpgConfig.GpgAgent.Config.Keys);
			}

			var actionDispatcher = container.Resolve<ActionDispatcher>();
			var hotkeyService = container.Resolve<HotkeyService>();
			
			// TODO: Not great, not terrible
			if (notifications is INotifyIcon n)
			{
				n.AddMenuActions(actionDispatcher);
			}
			AssignHotkeys(actionDispatcher, hotkeyService, notifications);

			if (container.Resolve<UpdateCheckingConfig>().CheckForUpdates)
			{
				container.Resolve<UpdateChecker>().Start();
			}

			container.Resolve<Option<RemoteUpdateChecker>>().Apply(c => c.Start());
		}

		/// <summary>
		/// Checks if all components are configured correctly.
		/// </summary>
		private static void RunInitialCheck(IContainer container, INotificationService notificationService)
		{
			var gpg = container.Resolve<GPG>();

			if (!Directory.Exists(ConfigManager.Config.PasswordStore.Location))
			{
				notificationService.ShowErrorWindow($"Could not find the password store at {Path.GetFullPath(ConfigManager.Config.PasswordStore.Location)}. Please make sure it exists.");
				App.Exit();
				return;
			}
			try
			{
				Log.Send("Using GPG version " + gpg.GetVersion());
			}
			catch (System.ComponentModel.Win32Exception)
			{
				notificationService.ShowErrorWindow("Could not find GPG. Make sure your gpg-path is set correctly.");
				App.Exit();
				return;
			}
			catch (Exception e)
			{
				notificationService.ShowErrorWindow($"Failed to initialise GPG. {e.GetType().Name}: {e.Message}");
				App.Exit();
				return;
			}
			if (ConfigManager.Config.Gpg.GpgAgent.Preload)
			{
				Task.Run(() =>
				{
					try
					{
						gpg.StartAgent();
					}
					catch (GpgError err)
					{
						notificationService.ShowErrorWindow(err.Message);
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
			ActionDispatcher actionDispatcher,
			HotkeyService hotkeyService,
			INotificationService notificationService)
		{
			try
			{
				hotkeyService.AssignHotkeys(
					ConfigManager.Config.Hotkeys ?? Array.Empty<HotkeyConfig>(),
					actionDispatcher,
					notificationService!);
			}
			catch (Exception e) when (e is HotkeyException)
			{
				Log.Send("Failed to register hotkeys", LogLevel.Error);
				Log.ReportException(e);

				notificationService!.ShowErrorWindow(e.Message, "Could not register hotkeys");
				App.Exit();
			}
		}
	}
}
