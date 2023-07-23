using PassWinmenu.Actions;
using PassWinmenu.UpdateChecking;

namespace PassWinmenu.Notifications
{
	internal interface INotifyIcon
	{
		void AddMenuActions(ActionDispatcher actionDispatcher);

		void AddUpdate(ProgramVersion version);
	}
}
