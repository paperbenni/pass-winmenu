using System.Net;
using Autofac;
using PassWinmenu.Configuration;
using PassWinmenu.WinApi;

namespace PassWinmenu;

internal sealed class Setup
{
	/// <summary>
	/// Loads all required resources.
	/// </summary>
	public static IContainer Initialise(INotificationService notificationService, IDialogService dialogService, ConfigManager configManager)
	{
		// Load compiled-in resources.
		EmbeddedResources.Load();

		Log.Send("------------------------------");
		Log.Send($"Starting pass-winmenu {Program.Version}");
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
}
