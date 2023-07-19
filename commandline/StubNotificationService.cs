using PassWinmenu.UpdateChecking;
using PassWinmenu.WinApi;

namespace PassWinmenu.CommandLine;

internal class StubNotificationService : INotificationService
{
	public void Dispose()
	{
	}

	public void Raise(string message, Severity level)
	{
	}

	public void HandleUpdateAvailable(UpdateAvailableEventArgs args)
	{
	}
}
