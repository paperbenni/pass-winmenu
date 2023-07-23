using Autofac;
using PassWinmenu.Configuration;
using PassWinmenu.Utilities;

namespace PassWinmenu.CommandLine;

using PasswordManagement;

public class ShowCommand
{
	private readonly string configPath;
	private readonly string passwordPath;
	private readonly IPasswordManager passwordManager;

	public ShowCommand(string configPath, string passwordPath)
	{
		if (!passwordPath.EndsWith(PassWinmenu.Program.EncryptedFileExtension))
		{
			passwordPath += PassWinmenu.Program.EncryptedFileExtension;
		}

		this.passwordPath = passwordPath;
		this.configPath = configPath;
		this.passwordManager = CreateContainer().Resolve<IPasswordManager>();
	}

	private IContainer CreateContainer()
	{
		var loadResult = ConfigManager.Load(configPath, allowCreate: false);
		var configManager = loadResult switch
		{
			LoadResult.Success s => s.ConfigManager,
			LoadResult.NeedsUpgrade => throw new Exception("Outdated configuration file"),
			LoadResult.NotFound => throw new Exception("Configuration file not found"),
			_ => throw new ArgumentOutOfRangeException(),
		};

		return Setup.InitialiseCommandLine(configManager);
	}

	public void All()
	{
		var password = GetPasswordFile();
		var decrypted = passwordManager.DecryptPassword(password, false).Content;
		Console.WriteLine(decrypted);
	}

	public void Password()
	{
		var password = GetPasswordFile();
		var decrypted = passwordManager.DecryptPassword(password, true).Password;
		Console.WriteLine(decrypted);
	}

	public void Key(string key)
	{
		var password = GetPasswordFile();
		var keys = passwordManager.DecryptPassword(password, true).Keys;

		var matchingPair = keys
			.Select<KeyValuePair<string, string>, KeyValuePair<string, string>?>(p => p)
			.FirstOrDefault(k => string.Equals(k!.Value.Key, key, StringComparison.CurrentCultureIgnoreCase));
		if (matchingPair.HasValue)
		{
			Console.WriteLine(matchingPair.Value.Value);
		}
		else
		{
			Console.Error.WriteLine("Key does not exist!");
			Environment.Exit(1);
		}
	}

	private PasswordFile GetPasswordFile()
	{
		var file = passwordManager.QueryPasswordFile(passwordPath).ValueOrDefault();
		if (file == null)
		{
			Console.Error.WriteLine("Password does not exist!");
			Environment.Exit(1);
		}

		return file;
	}
}

public static class Commands
{
	public static void ShowAll(string configPath, string passwordPath)
	{
		new ShowCommand(configPath, passwordPath).All();
	}

	public static void ShowPassword(string configPath, string passwordPath)
	{
		new ShowCommand(configPath, passwordPath).Password();
	}

	public static void ShowKey(string configPath, string passwordPath, string key)
	{
		new ShowCommand(configPath, passwordPath).Key(key);
	}
}
