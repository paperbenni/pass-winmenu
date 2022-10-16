using System;
using System.Windows;
using PassWinmenu.Windows;

#nullable enable
namespace PassWinmenu
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public sealed partial class App : Application, IDisposable
	{
		private static MainWindow? mainWindow;

		private void App_Startup(object sender, StartupEventArgs e)
		{
			mainWindow = new MainWindow();
			mainWindow.Start();
		}

		public void Dispose()
		{
			DisposeApplication();
		}

		private static void DisposeApplication()
		{
			mainWindow?.Dispose();
		}

		public new static void Exit()
		{
			mainWindow?.Close();
			DisposeApplication();
			Environment.Exit(0);
		}
	}
}
