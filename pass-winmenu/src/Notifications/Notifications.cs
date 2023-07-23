using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using PassWinmenu.Actions;
using PassWinmenu.Configuration;
using PassWinmenu.ExternalPrograms;
using PassWinmenu.UpdateChecking;
using PassWinmenu.WinApi;

namespace PassWinmenu.Notifications
{
	internal class Notifications : INotificationService, INotifyIcon, ISyncStateTracker
	{
		private readonly NotifyIcon icon;
		private readonly NotificationConfig config;
		private readonly ToolStripMenuItem downloadUpdate;
		private readonly ToolStripSeparator downloadSeparator;

		private const string DownloadUpdateString = "https://github.com/geluk/pass-winmenu/releases";
		private const int ToolTipTimeoutMs = 5000;

		private Notifications(NotifyIcon icon, NotificationConfig config)
		{
			this.icon = icon;
			this.config = config;
			this.icon.Click += HandleIconClick;

			downloadUpdate = new ToolStripMenuItem("Download Update")
			{
				BackColor = Color.Beige,
				Visible = false,
			};
			downloadUpdate.Click += HandleDownloadUpdateClick;

			downloadSeparator = new ToolStripSeparator
			{
				Visible = false
			};
		}

		public static Notifications Create(NotificationConfig config)
		{
			var icon = new NotifyIcon
			{
				Icon = EmbeddedResources.Icon,
				Visible = true
			};


			return new Notifications(icon, config);
		}

		private void HandleIconClick(object? sender, EventArgs e)
		{
			var args = (MouseEventArgs)e;
			if (args.Button == MouseButtons.Left)
			{
				// Unfortunately, calling Show() here does not do what you'd expect.
				// It displays the menu in the wrong place, and the menu won't hide if you click outside it.
				// ShowContextMenu() does the right thing, but it's a private method,
				// so we have to resort to a bit of a hack to get it to work.
				var mi = typeof(NotifyIcon)
					.GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
				mi!.Invoke(icon, null);
			}
		}

		public void AddMenuActions(ActionDispatcher actionDispatcher)
		{
			var menu = new ContextMenuStrip();
			menu.Items.Add(new ToolStripLabel("pass-winmenu " + Program.Version));
			menu.Items.Add(new ToolStripSeparator());

			menu.Items.Add(downloadUpdate);
			menu.Items.Add(downloadSeparator);

			menu.Items.Add("Decrypt Password", null, (sender, args) => actionDispatcher.DecryptPassword(true, false, false));
			menu.Items.Add("Add new Password", null, (sender, args) => actionDispatcher.Dispatch(HotkeyAction.AddPassword));
			menu.Items.Add("Edit Password File", null, (sender, args) => actionDispatcher.Dispatch(HotkeyAction.EditPassword));
			menu.Items.Add("Generate TOTP Code", null, (sender, args) => actionDispatcher.GenerateTotpCode(true, false));
			menu.Items.Add(new ToolStripSeparator());
			menu.Items.Add("Push to Remote", null, (sender, args) => actionDispatcher.Dispatch(HotkeyAction.GitPush));
			menu.Items.Add("Pull from Remote", null, (sender, args) => actionDispatcher.Dispatch(HotkeyAction.GitPull));
			menu.Items.Add(new ToolStripSeparator());
			menu.Items.Add("Open Explorer", null, (sender, args) => actionDispatcher.Dispatch(HotkeyAction.OpenExplorer));
			menu.Items.Add("Open Shell", null, (sender, args) => Task.Run(() => actionDispatcher.Dispatch(HotkeyAction.OpenShell)));
			menu.Items.Add(new ToolStripSeparator());

			var dropDown = new ToolStripMenuItem("More Actions");
			dropDown.DropDownItems.Add("Check for Updates", null, (sender, args) => actionDispatcher.Dispatch(HotkeyAction.CheckForUpdates));
			dropDown.DropDownItems.Add("Edit Configuration", null, (sender, args) => actionDispatcher.Dispatch(HotkeyAction.EditConfiguration));
			dropDown.DropDownItems.Add("Re-Encrypt Password Store", null, (sender, args) => actionDispatcher.Dispatch(HotkeyAction.ReencryptPasswordStore));
			dropDown.DropDownItems.Add("View Log", null, (sender, args) => actionDispatcher.Dispatch(HotkeyAction.ViewLog));

			menu.Items.Add(dropDown);
			menu.Items.Add(new ToolStripSeparator());

			var startupLink = new StartupLink("pass-winmenu");
			var startWithWindows = new ToolStripMenuItem("Start with Windows")
			{
				Checked = startupLink.Exists
			};
			startWithWindows.Click += (sender, args) =>
			{
				var target = Application.ExecutablePath;
				var workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
				startupLink.Toggle(target, workingDirectory);
				startWithWindows.Checked = startupLink.Exists;
			};

			menu.Items.Add(startWithWindows);
			menu.Items.Add("About", null, (sender, args) => Process.Start("https://github.com/geluk/pass-winmenu#readme"));
			menu.Items.Add("Quit", null, (sender, args) => App.Exit());
			icon.ContextMenuStrip = menu;
		}

		public void AddUpdate(ProgramVersion version)
		{
			downloadUpdate.Text += $" ({version})";
			downloadUpdate.Visible = true;
			downloadSeparator.Visible = true;
		}

		public void Raise(string message, Severity level)
		{
			if (config.Enabled)
			{
				icon.ShowBalloonTip(ToolTipTimeoutMs, "pass-winmenu", message, GetIconForSeverity(level));
			}
		}

		private ToolTipIcon GetIconForSeverity(Severity severity)
		{
			return severity switch
			{
				Severity.None => ToolTipIcon.None,
				Severity.Info => ToolTipIcon.Info,
				Severity.Warning => ToolTipIcon.Warning,
				Severity.Error => ToolTipIcon.Error,
				_ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null),
			};
		}

		private static void HandleDownloadUpdateClick(object? sender, EventArgs e)
		{
			Process.Start(DownloadUpdateString);
		}

		public void Dispose()
		{
			icon.Dispose();
			downloadUpdate.Dispose();
			downloadSeparator.Dispose();
		}

		public void SetSyncState(SyncState state)
		{
			icon.Icon = state switch
			{
				SyncState.UpToDate => EmbeddedResources.Icon,
				SyncState.Ahead => EmbeddedResources.IconAhead,
				SyncState.Behind => EmbeddedResources.IconBehind,
				SyncState.Diverged => EmbeddedResources.IconDiverged,
				_ => icon.Icon
			};
		}
	}

	internal enum Severity
	{
		None,
		Info,
		Warning,
		Error,
	}
}
