using PassWinmenu.Actions;

namespace PassWinmenu.WinApi
{
	internal interface INotifyIcon
	{
		void AddMenuActions(ActionDispatcher actionDispatcher);
	}
}
