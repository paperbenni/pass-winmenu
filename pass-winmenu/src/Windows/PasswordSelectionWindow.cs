using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace PassWinmenu.Windows
{
	internal class PasswordSelectionWindow<TEntry> : SelectionWindow
	{
		private readonly Dictionary<string, TEntry> entries;

		public PasswordSelectionWindow(IEnumerable<TEntry> options, Func<TEntry, string> keySelector, SelectionWindowConfiguration configuration) : base(configuration)
		{
			entries = options.ToDictionary(keySelector);
			ResetLabels(entries.Keys);
		}

		public TEntry Selection => entries[SelectedLabel.Text];

		protected override void OnSearchTextChanged(object sender, TextChangedEventArgs e)
		{
			// We split on spaces to allow the user to quickly search for a certain term, as it allows them
			// to search, for example, for site.com/username by entering "si us"
			var terms = SearchBox.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			var matching = entries.Keys.Where((key) =>
			{
				var lcOption = key.ToLower(CultureInfo.CurrentCulture);
				return terms.All(term =>
				{
					// Perform case-sensitive matching if the user entered an uppercase character.
					if (term.Any(char.IsUpper))
					{
						if (key.Contains(term)) return true;
					}
					else
					{
						if (lcOption.Contains(term)) return true;
					}
					return false;
				});
			});
			ResetLabels(matching);
		}

		protected override void HandleSelect()
		{
			if (SelectedLabel != null)
			{
				Success = true;
				Close();
			}
		}
	}
}
