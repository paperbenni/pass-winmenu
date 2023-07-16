using PassWinmenu.UpdateChecking;

namespace PassWinmenu.WinApi;

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
