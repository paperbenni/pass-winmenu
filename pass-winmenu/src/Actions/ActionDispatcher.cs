using System;
using System.Collections.Generic;
using PassWinmenu.Configuration;
using PassWinmenu.Windows;

#nullable enable
namespace PassWinmenu.Actions
{
	class ActionDispatcher
	{
		private readonly DialogCreator dialogCreator;
		private readonly DecryptPasswordAction decryptPasswordAction;
		private readonly Dictionary<HotkeyAction, IAction> actions;

		public ActionDispatcher(
			DialogCreator dialogCreator,
			DecryptPasswordAction decryptPasswordAction,
			Dictionary<HotkeyAction, IAction> actions)
		{
			this.dialogCreator = dialogCreator;
			this.decryptPasswordAction = decryptPasswordAction;
			this.actions = actions;
		}

		/// <summary>
		/// Asks the user to choose a password file, decrypts it, and copies the resulting value to the clipboard.
		/// </summary>
		public void DecryptPassword(bool copyToClipboard, bool typeUsername, bool typePassword)
		{
			decryptPasswordAction.Execute(copyToClipboard, typeUsername, typePassword);
		}

		/// <summary>
		/// Asks the user to choose a password file, decrypts it,
		/// generates an OTP code from the secret in the totp field, and copies the resulting value to the clipboard.
		/// </summary>
		public void GenerateTotpCode(bool copyToClipboard, bool typeTotpCode)
		{
			dialogCreator.GenerateTotpCode(copyToClipboard, typeTotpCode);
		}

		public void DecryptMetadata(bool copyToClipboard, bool type)
		{
			dialogCreator.DecryptMetadata(copyToClipboard, type);
		}

		public void DecryptPasswordField(bool copyToClipboard, bool type, string? fieldName = null)
		{
			dialogCreator.GetKey(copyToClipboard, type, fieldName);
		}

		public void Dispatch(HotkeyAction hotkeyAction)
		{
			if (actions.TryGetValue(hotkeyAction, out var action))
			{
				action.Execute();
			}
			else
			{
				throw new NotImplementedException("Action does not exist.");
			}
		}
	}
}
