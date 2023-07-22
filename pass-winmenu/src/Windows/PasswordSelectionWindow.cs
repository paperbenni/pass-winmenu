using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using PassWinmenu.Configuration;
using PassWinmenu.Utilities;

namespace PassWinmenu.Windows
{
	internal class PasswordSelectionWindow<TEntry> : SelectionWindow
	{
		private readonly Dictionary<string, TEntry> entries;

		public PasswordSelectionWindow(
			IEnumerable<TEntry> options,
			Func<TEntry, string> keySelector,
			SelectionWindowConfiguration configuration,
			InterfaceConfig interfaceConfig,
			string hint)
			: base(configuration, interfaceConfig, hint)
		{
			entries = options.ToDictionary(keySelector);
			ResetItems(entries.Keys);
		}

		public TEntry Selection => entries[SelectionText];

		protected override void OnSearchTextChanged(object sender, TextChangedEventArgs e)
		{
			var matches = Search.Match(entries.Keys, SearchBox.Text);
			ResetItems(matches);
		}

		protected override void HandleConfirm()
		{
			Success = true;
			Close();
		}
	}
}
