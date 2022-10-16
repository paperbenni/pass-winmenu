using System.Diagnostics;
using PassWinmenu.Configuration;

namespace PassWinmenu.Actions
{
	internal class EditConfigurationAction : IAction
	{
		private readonly RuntimeConfiguration runtimeConfiguration;

		public EditConfigurationAction(RuntimeConfiguration runtimeConfiguration)
		{
			this.runtimeConfiguration = runtimeConfiguration;
		}

		public void Execute()
		{
			var startInfo = new ProcessStartInfo
			{
				FileName = "explorer", 
				Arguments = runtimeConfiguration.ConfigFileLocation,
			};
			Process.Start(startInfo);
		}

		public HotkeyAction ActionType => HotkeyAction.EditConfiguration;
	}
}
