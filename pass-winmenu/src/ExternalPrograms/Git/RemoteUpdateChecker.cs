using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Navigation;
using PassWinmenu.Configuration;
using PassWinmenu.WinApi;

namespace PassWinmenu.ExternalPrograms
{
	internal class RemoteUpdateChecker: IDisposable
	{
		private readonly ISyncService syncService;
		private readonly GitConfig gitConfig;
		private readonly ISyncStateTracker syncStateTracker;
		private Timer timer;
		private TimeSpan CheckInterval { get; } = TimeSpan.FromSeconds(10);

		public RemoteUpdateChecker(ISyncService syncService, GitConfig gitConfig, ISyncStateTracker syncStateTracker)
		{
			this.syncService = syncService;
			this.gitConfig = gitConfig;
			this.syncStateTracker = syncStateTracker;
		}

		/// <summary>
		/// Starts checking for updates.
		/// </summary>
		public void Start()
		{
			timer = new Timer(CheckInterval.TotalMilliseconds)
			{
				AutoReset = true,
			};

			timer.Elapsed += (sender, args) =>
			{
				CheckForUpdates();
			};
			timer.Start();
		}

		public void CheckForUpdates()
		{
			try
			{
				syncService.Fetch();
			}
			catch (Exception e)
			{
				Log.Send("Failed to fetch latest updates from remote.");
				Log.ReportException(e);
			}

			var trackingDetails = syncService.GetTrackingDetails();
			if (trackingDetails.BehindBy > 0 && trackingDetails.AheadBy > 0)
			{
				syncStateTracker.SetSyncState(SyncState.Diverged);
			}
			else if (trackingDetails.BehindBy > 0)
			{
				syncStateTracker.SetSyncState(SyncState.Behind);
			} 
			else if (trackingDetails.AheadBy > 0)
			{
				syncStateTracker.SetSyncState(SyncState.Ahead);
			}
			else
			{
				syncStateTracker.SetSyncState(SyncState.UpToDate);
			}
		}

		public void Dispose()
		{
			timer?.Dispose();
		}
	}

	internal enum SyncState
	{
		UpToDate,
		Ahead,
		Behind,
		Diverged
	}

	internal interface ISyncStateTracker
	{
		void SetSyncState(SyncState state);
	}
}
