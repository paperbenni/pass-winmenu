using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PassWinmenu.Utilities;

#nullable enable
namespace PassWinmenu.Windows
{
	internal class FileSelectionWindow : SelectionWindow
	{
		private readonly DirectoryAutocomplete autocomplete;
		private readonly IDirectoryInfo baseDirectory;

		public FileSelectionWindow(IDirectoryInfo baseDirectory, SelectionWindowConfiguration configuration, string hint) : base(configuration, hint)
		{
			this.baseDirectory = baseDirectory;
			autocomplete = new DirectoryAutocomplete(baseDirectory);
			var completions = autocomplete.GetCompletionList("");
			ResetLabels(completions);
		}

		protected override void OnSearchTextChanged(object sender, TextChangedEventArgs e)
		{
			var completions = autocomplete.GetCompletionList(SearchBox.Text).ToList();
			if (!string.IsNullOrWhiteSpace(SearchBox.Text)
				&& !SearchBox.Text.EndsWith("/", StringComparison.Ordinal)
				&& !completions.Contains(SearchBox.Text))
			{
				completions.Insert(0, SearchBox.Text);
			}
			ResetLabels(completions);
		}

		protected override void HandleSelect()
		{
			if (SelectedLabel == null) return;

			var selection = SelectionText;
			if (selection == null) return;

			// If a suggestion is selected, put that suggestion in the searchbox.
			if (Options.IndexOf(SelectedLabel) > 0 || string.IsNullOrEmpty(SearchBox.Text))
			{
				if (selection.EndsWith(Program.EncryptedFileExtension, StringComparison.Ordinal))
				{
					selection = selection.Substring(0, selection.Length - 4);
				}
				SetSearchBoxText(selection);
			}
			else
			{
				if (SearchBox.Text.EndsWith("/", StringComparison.Ordinal))
				{
					return;
				}
				if (selection.EndsWith(Program.EncryptedFileExtension, StringComparison.Ordinal))
				{
					MessageBox.Show("A .gpg extension will be added automatically and does not need to be entered here.");
					selection = selection.Substring(0, selection.Length - 4);
					SetSearchBoxText(selection);
					return;
				}
				if (File.Exists(Path.Combine(baseDirectory.FullName, selection + Program.EncryptedFileExtension)))
				{
					MessageBox.Show($"The password file \"{selection + Program.EncryptedFileExtension}\" already exists.");
					return;
				}
				Success = true;
				Close();
			}
		}
	}
}
