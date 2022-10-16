using System;
using System.Windows;

#nullable enable
namespace PassWinmenu.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public sealed partial class MainWindow : Window, IDisposable
	{
		private IDisposable? program;

		public MainWindow()
		{
			InitializeComponent();
		}

		public void Start()
		{
			program = Program.Start();
		}

		public void Dispose()
		{
			program?.Dispose();
		}
	}
}
