using System;
using System.Diagnostics;
using PassWinmenu.WinApi;
using YamlDotNet.Core;

namespace PassWinmenu.Configuration;

internal class ConfigurationLoader
{
	public static void Load(INotificationService notificationService)
	{
		var configFileLocation = "pass-winmenu.yaml";

		LoadResult result;
		try
		{
			result = ConfigManager.Load(configFileLocation);
		}
		catch (Exception e) when (e.InnerException != null)
		{
			if (e is YamlException)
			{
				notificationService.ShowErrorWindow(
					$"The configuration file could not be loaded: {e.Message}\n\n"
					+ $"{e.InnerException.GetType().Name}: {e.InnerException.Message}",
					"Unable to load configuration file.");
			}
			else
			{
				notificationService.ShowErrorWindow(
					$"The configuration file could not be loaded. An unhandled exception occurred.\n"
					+ $"{e.InnerException.GetType().Name}: {e.InnerException.Message}",
					"Unable to load configuration file.");
			}

			App.Exit();
			return;
		}
		catch (SemanticErrorException e)
		{
			notificationService.ShowErrorWindow(
				$"The configuration file could not be loaded, a YAML error was encountered.\n"
				+ $"{e.GetType().Name}: {e.Message}\n\n"
				+ $"File location: {configFileLocation}",
				"Unable to load configuration file.");
			App.Exit();
			return;
		}
		catch (YamlException e)
		{
			notificationService.ShowErrorWindow(
				$"The configuration file could not be loaded. An unhandled exception occurred.\n{e.GetType().Name}: {e.Message}",
				"Unable to load configuration file.");
			App.Exit();
			return;
		}

		switch (result)
		{
			case LoadResult.FileCreationFailure:
				notificationService.Raise(
					"A default configuration file was generated, but could not be saved.\nPass-winmenu will fall back to its default settings.",
					Severity.Error);
				break;
			case LoadResult.NewFileCreated:
				var open = notificationService.ShowYesNoWindow(
					"A new configuration file has been generated. Please modify it according to your preferences and restart the application.\n\n"
					+ "Would you like to open it now?",
					"New configuration file created");
				if (open)
				{
					Process.Start("explorer", configFileLocation);
				}

				App.Exit();
				return;
			case LoadResult.NeedsUpgrade:
				var backedUpFile = ConfigManager.Backup(configFileLocation);
				var openBoth = notificationService.ShowYesNoWindow(
					"The current configuration file is out of date. A new configuration file has been created, and the old file has been backed up.\n"
					+ "Please edit the new configuration file according to your preferences and restart the application.\n\n"
					+ "Would you like to open both files now?",
					"Configuration file out of date");
				if (openBoth)
				{
					Process.Start("explorer", configFileLocation);
					Process.Start(backedUpFile);
				}

				App.Exit();
				return;
		}

		if (ConfigManager.Config.Application.ReloadConfig)
		{
			ConfigManager.EnableAutoReloading(configFileLocation);
			Log.Send("Config reloading enabled");
		}
	}
}
