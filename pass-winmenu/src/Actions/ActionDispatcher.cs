using System;
using System.Collections.Generic;
using PassWinmenu.Configuration;
using PassWinmenu.WinApi;

#nullable enable
namespace PassWinmenu.Actions
{
	internal class ActionDispatcher
	{
		private readonly DecryptPasswordAction decryptPasswordAction;
		private readonly GenerateTotpAction generateTotpAction;
		private readonly DecryptMetadataAction decryptMetadataAction;
		private readonly GetKeyAction getKeyAction;
		private readonly IDialogService dialogService;
		private readonly Dictionary<HotkeyAction, IAction> actions;

		public ActionDispatcher(
			DecryptPasswordAction decryptPasswordAction,
			GenerateTotpAction generateTotpAction,
			DecryptMetadataAction decryptMetadataAction,
			GetKeyAction getKeyAction,
			IDialogService dialogService,
			Dictionary<HotkeyAction, IAction> actions)
		{
			this.decryptPasswordAction = decryptPasswordAction;
			this.generateTotpAction = generateTotpAction;
			this.decryptMetadataAction = decryptMetadataAction;
			this.getKeyAction = getKeyAction;
			this.dialogService = dialogService;
			this.actions = actions;
		}

		/// <summary>
		/// Asks the user to choose a password file, decrypts it, and copies the resulting value to the clipboard.
		/// </summary>
		public void DecryptPassword(bool copyToClipboard, bool typeUsername, bool typePassword)
		{
			Try(() => decryptPasswordAction.Execute(copyToClipboard, typeUsername, typePassword), "Unable to decrypt password");
		}

		/// <summary>
		/// Asks the user to choose a password file, decrypts it,
		/// generates an OTP code from the secret in the totp field, and copies the resulting value to the clipboard.
		/// </summary>
		public void GenerateTotpCode(bool copyToClipboard, bool typeTotpCode)
		{
			Try(() => generateTotpAction.GenerateTotpCode(copyToClipboard, typeTotpCode), "Unable to generate TOTP code");
		}

		public void DecryptMetadata(bool copyToClipboard, bool type)
		{
			Try(() => decryptMetadataAction.DecryptMetadata(copyToClipboard, type), "Unable to decrypt metadata");
		}

		public void DecryptPasswordField(bool copyToClipboard, bool type, string? fieldName = null)
		{
			Try(() => getKeyAction.GetKey(copyToClipboard, type, fieldName), "Unable to decrypt password field");
		}

		public void Dispatch(HotkeyAction hotkeyAction)
		{
			if (actions.TryGetValue(hotkeyAction, out var action))
			{
				Try(() => action.Execute(), $"Action '{hotkeyAction}' failed");
			}
			else
			{
				dialogService.ShowErrorWindow($"No handler for action '{hotkeyAction}' exists");
			}
		}

		private void Try(Action action, string baseMessage)
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				dialogService.ShowErrorWindow($"{baseMessage}: " + e.Message);
			}
		}
	}
}
