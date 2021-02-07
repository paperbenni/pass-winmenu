using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using OtpNet;
using PassWinmenu.Configuration;
using PassWinmenu.ExternalPrograms.Gpg;
using PassWinmenu.PasswordManagement;
using PassWinmenu.Utilities;
using PassWinmenu.WinApi;

#nullable enable
namespace PassWinmenu.Windows
{
	internal class DialogCreator
	{
		private readonly INotificationService notificationService;
		private readonly IPasswordManager passwordManager;
		private readonly PathDisplayHelper pathDisplayHelper;

		public DialogCreator(
			INotificationService notificationService,
			IPasswordManager passwordManager,
			PathDisplayHelper pathDisplayHelper)
		{
			this.notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
			this.passwordManager = passwordManager ?? throw new ArgumentNullException(nameof(passwordManager));
			this.pathDisplayHelper = pathDisplayHelper;
		}

		public void DecryptMetadata(bool copyToClipboard, bool type)
		{
			var selectedFile = RequestPasswordFile();
			if (selectedFile == null)
			{
				return;
			}
			KeyedPasswordFile passFile;
			try
			{
				passFile = passwordManager.DecryptPassword(selectedFile, ConfigManager.Config.PasswordStore.FirstLineOnly);
			}
			catch (GpgError e)
			{
				notificationService.ShowErrorWindow("Password decryption failed: " + e.Message);
				return;
			}
			catch (GpgException e)
			{
				notificationService.ShowErrorWindow("Password decryption failed. " + e.Message);
				return;
			}
			catch (ConfigurationException e)
			{
				notificationService.ShowErrorWindow("Password decryption failed: " + e.Message);
				return;
			}
			catch (Exception e)
			{
				notificationService.ShowErrorWindow($"Password decryption failed: An error occurred: {e.GetType().Name}: {e.Message}");
				return;
			}

			if (copyToClipboard)
			{
				ClipboardHelper.Place(passFile.Metadata, TimeSpan.FromSeconds(ConfigManager.Config.Interface.ClipboardTimeout));
				if (ConfigManager.Config.Notifications.Types.PasswordCopied)
				{
					notificationService.Raise($"The key has been copied to your clipboard.\nIt will be cleared in {ConfigManager.Config.Interface.ClipboardTimeout:0.##} seconds.", Severity.Info);
				}
			}
			if (type)
			{
				KeyboardEmulator.EnterText(passFile.Metadata);
			}
		}

		/// <summary>
		/// Asks the user to choose a password file, decrypts it,
		/// generates an OTP code from the secret in the totp field, and copies the resulting value to the clipboard.
		/// </summary>
		public void GenerateTotpCode(bool copyToClipboard, bool typeTotpCode)
		{
			var selectedFile = RequestPasswordFile();
			// If the user cancels their selection, the password decryption should be cancelled too.
			if (selectedFile == null) return;

			KeyedPasswordFile passFile;
			try
			{
				passFile = passwordManager.DecryptPassword(selectedFile, true);
			}
			catch (Exception e) when (e is GpgError || e is GpgException || e is ConfigurationException)
			{
				notificationService.ShowErrorWindow("TOTP decryption failed: " + e.Message);
				return;
			}
			catch (Exception e)
			{
				notificationService.ShowErrorWindow($"TOTP decryption failed: An error occurred: {e.GetType().Name}: {e.Message}");
				return;
			}

			String secretKey = null;

			foreach (var k in passFile.Keys)
			{
				// totp: Xxxx4
				if (k.Key == "totp")
				{
					secretKey = k.Value;
				}

				// otpauth: //totp/account?secret=FxxxX&digits=6
				if (k.Key == "otpauth")
				{
					var regex = new Regex("secret=([a-zA-Z0-9]+)&", RegexOptions.Compiled | RegexOptions.IgnoreCase);
					var matches = regex.Match(k.Value);
					if (matches.Success) {
						secretKey = matches.Groups[1].Value;
					}
				}
			}

			var foo = String.Join(",", passFile.Keys.Select(f => f.Key));

			if (secretKey == null)
			{
				notificationService.ShowErrorWindow($"TOTP decryption failed: Failed to find an OTP secret. Keys = ${foo}");
				return;
			}

			var secretKeyBytes = Base32Encoding.ToBytes(secretKey);
			var totp = new Totp(secretKeyBytes);
			var totpCode = totp.ComputeTotp();

			if (copyToClipboard)
			{
				ClipboardHelper.Place(totpCode, TimeSpan.FromSeconds(ConfigManager.Config.Interface.ClipboardTimeout));
				if (ConfigManager.Config.Notifications.Types.TotpCopied)
				{
					notificationService.Raise($"The totp code has been copied to your clipboard.\nIt will be cleared in {ConfigManager.Config.Interface.ClipboardTimeout:0.##} seconds.", Severity.Info);
				}
			}

			if (typeTotpCode)
			{
				KeyboardEmulator.EnterText(totpCode);
			}
		}

		public void GetKey(bool copyToClipboard, bool type, string? key)
		{
			var selectedFile = RequestPasswordFile();
			if (selectedFile == null) return;

			KeyedPasswordFile passFile;
			try
			{
				passFile = passwordManager.DecryptPassword(selectedFile, ConfigManager.Config.PasswordStore.FirstLineOnly);
			}
			catch (GpgError e)
			{
				notificationService.ShowErrorWindow("Password decryption failed: " + e.Message);
				return;
			}
			catch (GpgException e)
			{
				notificationService.ShowErrorWindow("Password decryption failed. " + e.Message);
				return;
			}
			catch (ConfigurationException e)
			{
				notificationService.ShowErrorWindow("Password decryption failed: " + e.Message);
				return;
			}
			catch (Exception e)
			{
				notificationService.ShowErrorWindow($"Password decryption failed: An error occurred: {e.GetType().Name}: {e.Message}");
				return;
			}

			if (string.IsNullOrWhiteSpace(key))
			{
				key = ShowPasswordMenu(passFile.Keys, k => k.Key, "Choose a key...").ValueOrDefault().Key;
				if (key == null)
				{
					return;
				}
			}

			var values = passFile.Keys.Where(k => k.Key == key).ToList();
			if (values.Count == 0)
			{
				return;
			}

			string chosenValue;
			if (values.Count > 1)
			{
				var choice = ShowPasswordMenu(values, v => v.Value, "Multiple keys found, choose a value...");
				if (choice.IsNone)
				{
					return;
				}

				chosenValue = choice.ValueOrDefault().Value;
			}
			else
			{
				chosenValue = values[0].Value;
			}

			if (copyToClipboard)
			{
				ClipboardHelper.Place(chosenValue, TimeSpan.FromSeconds(ConfigManager.Config.Interface.ClipboardTimeout));
				if (ConfigManager.Config.Notifications.Types.PasswordCopied)
				{
					notificationService.Raise($"The key has been copied to your clipboard.\nIt will be cleared in {ConfigManager.Config.Interface.ClipboardTimeout:0.##} seconds.", Severity.Info);
				}
			}
			if (type)
			{
				KeyboardEmulator.EnterText(chosenValue);
			}
		}

		/// <summary>
		/// Opens a window where the user can choose the location for a new password file.
		/// </summary>
		/// <returns>The path to the file that the user has chosen</returns>
		public string? ShowFileSelectionWindow()
		{
			SelectionWindowConfiguration windowConfig;
			try
			{
				windowConfig = SelectionWindowConfiguration.ParseMainWindowConfiguration(ConfigManager.Config);
			}
			catch (ConfigurationParseException e)
			{
				notificationService.Raise(e.Message, Severity.Error);
				return null;
			}

			// Ask the user where the password file should be placed.
			var pathWindow = new FileSelectionWindow(passwordManager.PasswordStore, windowConfig, "Choose a location...");
			pathWindow.ShowDialog();
			if (!pathWindow.Success)
			{
				return null;
			}
			return pathWindow.SelectionText + Program.EncryptedFileExtension;
		}

		/// <summary>
		/// Asks the user to choose a password file.
		/// </summary>
		/// <returns>
		/// The path to the chosen password file (relative to the password directory),
		/// or null if the user didn't choose anything.
		/// </returns>
		public PasswordFile? RequestPasswordFile()
		{
			Helpers.AssertOnUiThread();

			// Find GPG-encrypted password files
			var passFiles = passwordManager.GetPasswordFiles().ToList();
			if (passFiles.Count == 0)
			{
				MessageBox.Show("Your password store doesn't appear to contain any passwords yet.", "Empty password store", MessageBoxButton.OK, MessageBoxImage.Information);
				return null;
			}
			return ShowPasswordMenu(passFiles, pathDisplayHelper.GetDisplayPath, "Select a password...").ValueOrDefault();
		}



		/// <summary>
		/// Opens the password menu and displays it to the user, allowing them to choose an existing password file.
		/// </summary>
		/// <param name="options">A list of options the user can choose from.</param>
		/// <param name="keySelector">A function that selects a string to display for each option in <paramref name="options"/>.</param>
		/// <param name="hint">A search hint to show, or null if no hint should be shown.</param>
		/// <returns>One of the values contained in <paramref name="options"/>, or the default value of <typeparamref name="TEntry"/> if no option was chosen.</returns>
		public Option<TEntry> ShowPasswordMenu<TEntry>(IEnumerable<TEntry> options, Func<TEntry, string> keySelector, string hint)
		{
			SelectionWindowConfiguration windowConfig;
			try
			{
				windowConfig = SelectionWindowConfiguration.ParseMainWindowConfiguration(ConfigManager.Config);
			}
			catch (ConfigurationParseException e)
			{
				notificationService.Raise(e.Message, Severity.Error);
				return default;
			}

			var menu = new PasswordSelectionWindow<TEntry>(options, keySelector, windowConfig, hint);
			menu.ShowDialog();
			if (menu.Success)
			{
				return menu.Selection;
			}

			return Option<TEntry>.None;
		}

	}
}
