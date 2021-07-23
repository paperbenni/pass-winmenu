using System;
using System.Windows;
using PassWinmenu.UpdateChecking;

namespace PassWinmenu.WinApi
{
	internal interface INotificationService : IDisposable
	{
		void Raise(string message, Severity level);
		void ShowErrorWindow(string message, string title = "An error occurred.");

		bool ShowYesNoWindow(string message, string title, MessageBoxImage image = MessageBoxImage.None);

		void HandleUpdateAvailable(UpdateAvailableEventArgs args);
	}
}
