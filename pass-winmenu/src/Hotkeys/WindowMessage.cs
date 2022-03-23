namespace PassWinmenu.Hotkeys
{
	/// <summary>
	/// The types of message the window can receive. This enumeration
	/// does not exhaustively list message types.
	/// </summary>
	public enum WindowMessage : uint
	{
		/// <summary>
		/// The message received when a global hotkey is triggered.
		/// </summary>
		Hotkey = 0x0312,
	}
}
