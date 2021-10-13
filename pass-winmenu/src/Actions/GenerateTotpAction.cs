using System;
using System.Text.RegularExpressions;
using OtpNet;
using PassWinmenu.Configuration;
using PassWinmenu.ExternalPrograms.Gpg;
using PassWinmenu.PasswordManagement;
using PassWinmenu.WinApi;
using PassWinmenu.Windows;

#nullable enable
namespace PassWinmenu.Actions
{
	class GenerateTotpAction
	{
		private readonly IPasswordManager passwordManager;
		private readonly INotificationService notificationService;
		private readonly DialogCreator dialogCreator;
		private readonly InterfaceConfig config;

		private static readonly Regex OtpSecretRegex = new Regex("secret=([a-zA-Z0-9]+)&", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public GenerateTotpAction(
			IPasswordManager passwordManager,
			INotificationService notificationService,
			DialogCreator dialogCreator,
			InterfaceConfig config)
		{
			this.passwordManager = passwordManager;
			this.notificationService = notificationService;
			this.dialogCreator = dialogCreator;
			this.config = config;
		}

		/// <summary>
		/// Asks the user to choose a password file, decrypts it,
		/// generates an OTP code from the secret in the totp field, and copies the resulting value to the clipboard.
		/// </summary>
		public void GenerateTotpCode(bool copyToClipboard, bool typeTotpCode)
		{
			var selectedFile = dialogCreator.RequestPasswordFile();
			// If the user cancels their selection, the password decryption should be cancelled too.
			if (selectedFile == null) return;

			KeyedPasswordFile passwordFile;
			try
			{
				passwordFile = passwordManager.DecryptPassword(selectedFile, true);
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

			string? secretKey = null;
			foreach (var k in passwordFile.Keys)
			{
				// totp: Xxxx4
				if (k.Key.ToUpperInvariant() == "TOTP")
				{
					secretKey = k.Value;
				}

				// otpauth: //totp/account?secret=FxxxX&digits=6
				if (k.Key.ToUpperInvariant() == "OTPAUTH")
				{
					var match = OtpSecretRegex.Match(k.Value);
					if (match.Success) {
						secretKey = match.Groups[1].Value;
					}
				}
			}

			if (secretKey == null)
			{
				notificationService.ShowErrorWindow($"TOTP decryption failed: Failed to find an OTP secret.");
				return;
			}

			var secretKeyBytes = Base32Encoding.ToBytes(secretKey);
			var totp = new Totp(secretKeyBytes);
			var totpCode = totp.ComputeTotp();

			if (copyToClipboard)
			{
				ClipboardHelper.Place(totpCode, TimeSpan.FromSeconds(config.ClipboardTimeout));
				if (ConfigManager.Config.Notifications.Types.TotpCopied)
				{
					notificationService.Raise($"The totp code has been copied to your clipboard.\nIt will be cleared in {config.ClipboardTimeout:0.##} seconds.", Severity.Info);
				}
			}

			if (typeTotpCode)
			{
				KeyboardEmulator.EnterText(totpCode);
			}
		}
	}
}
