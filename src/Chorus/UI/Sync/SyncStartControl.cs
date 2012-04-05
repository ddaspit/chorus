﻿using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Chorus.Properties;
using Chorus.UI.Misc;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using System.Linq;
using Palaso.Code;
using Palaso.Progress.LogBox;

namespace Chorus.UI.Sync
{
	internal partial class SyncStartControl : UserControl
	{
		private HgRepository _repository;
		private SyncStartModel _model;
		public event EventHandler<SyncStartArgs> RepositoryChosen;
		private const string _connectionDiagnostics = "There was a problem connecting to the {0}.\r\n{1}Connection attempt failed.";

		private Thread _updateInternetSituation;
		private InternetStateWorker _internetStateWorker;

		/// <summary>
		/// Set this flag to get the FLEx-preferred behavior of enabling the buttons all the time
		/// (if not configured, clicking will launch configure dialog and then do the Send/Receive).
		/// </summary>
		public bool AlwaysEnableInternetAndLanButtons { get; set; }

		//designer only
		public SyncStartControl()
		{
			InitializeComponent();
		}
		public SyncStartControl(HgRepository repository)
		{
			InitializeComponent();
			Init(repository);
		}

		public void Init(HgRepository repository)
		{
			Guard.AgainstNull(repository, "repository");
			SetupSharedFolderAndInternetUI();

			_model = new SyncStartModel(repository);
			_repository = repository;
			_updateDisplayTimer.Enabled = true;
			_userName.Text = repository.GetUserIdInUse();
			// let the dialog display itself first, then check for connection
			_updateDisplayTimer.Interval = 500; // But check sooner than 2 seconds anyway!

			// Setup Internet State Checking thread and worker
			_internetStateWorker = new InternetStateWorker(CheckInternetStatusAndSetUI);
			_updateInternetSituation = new Thread(_internetStateWorker.DoWork);
		}

		private void SetupSharedFolderAndInternetUI()
		{
			const string checkingConnection = "Checking connection...";
			_useSharedFolderStatusLabel.Text = checkingConnection;
			_useSharedFolderButton.Enabled = false;

			_internetStatusLabel.Text = checkingConnection;
			_useInternetButton.Enabled = false;
		}

		private void OnUpdateDisplayTick(object sender, EventArgs e)
		{
			UpdateDisplay();
			_updateDisplayTimer.Interval = 2000; // more normal checking rate from here on out
		}

		private void UpdateDisplay()
		{
			UpdateUsbDriveSituation();
			UpdateInternetSituation();
			UpdateLocalNetworkSituation();
		}

		private void UpdateLocalNetworkSituation()
		{
			string message, tooltip, diagnostics;
			_useSharedFolderButton.Enabled = _model.GetNetworkStatusLink(out message, out tooltip, out diagnostics);

			if (!string.IsNullOrEmpty(diagnostics))
				SetupSharedFolderDiagnosticLink(diagnostics);
			else
				_sharedNetworkDiagnosticsLink.Visible = false;

			_useSharedFolderStatusLabel.Text = message;
			_useSharedFolderStatusLabel.LinkArea = new LinkArea(message.Length + 1, 1000);
			if (_useSharedFolderButton.Enabled)
			{
				tooltip += System.Environment.NewLine + "Press Shift to see Set Up button";
			}
			toolTip1.SetToolTip(_useSharedFolderButton, tooltip);

			if (!_useSharedFolderButton.Enabled || Control.ModifierKeys == Keys.Shift)
			{
				_useSharedFolderStatusLabel.Text += " Set Up";
			}
		}

		private void SetupSharedFolderDiagnosticLink(string diagnosticText)
		{
			_sharedNetworkDiagnosticsLink.Tag = diagnosticText;
			_sharedNetworkDiagnosticsLink.Enabled = _sharedNetworkDiagnosticsLink.Visible = true;
		}

		private void UpdateInternetSituation()
		{
			if (!_updateInternetSituation.IsAlive)
			{
				_updateInternetSituation.Start();
			}
		}

		private void CheckInternetStatusAndSetUI()
		{
			string buttonLabel, message, tooltip, diagnostics;
			bool result = _model.GetInternetStatusLink(out buttonLabel, out message, out tooltip,
													   out diagnostics);
			_useInternetButton.Enabled = result;
			if (!string.IsNullOrEmpty(diagnostics))
				SetupInternetDiagnosticLink(diagnostics);
			else
				_internetDiagnosticsLink.Visible = false;

			_useInternetButton.Text = buttonLabel;
			_internetStatusLabel.Text = message;
			_internetStatusLabel.LinkArea = new LinkArea(message.Length + 1, 1000);
			if (_useInternetButton.Enabled)
				tooltip += System.Environment.NewLine + "Press Shift to see Set Up button";
			toolTip1.SetToolTip(_useInternetButton, tooltip);

			if (!_useInternetButton.Enabled || Control.ModifierKeys == Keys.Shift)
				_internetStatusLabel.Text += " Set Up";
		}

		private void SetupInternetDiagnosticLink(string diagnosticText)
		{
			_internetDiagnosticsLink.Tag = diagnosticText;
			_internetDiagnosticsLink.Enabled = _internetDiagnosticsLink.Visible = true;
		}

		private void UpdateUsbDriveSituation()
		{
			// usbDriveLocator is defined in the Designer
			string message;
			_useUSBButton.Enabled = _model.GetUsbStatusLink(usbDriveLocator, out message);
			_usbStatusLabel.Text = message;
		}

		private void _useUSBButton_Click(object sender, EventArgs e)
		{
			if (RepositoryChosen != null)
			{
				UpdateName();
				var address = RepositoryAddress.Create(RepositoryAddress.HardWiredSources.UsbKey, "USB flash drive", false);
				RepositoryChosen.Invoke(this, new SyncStartArgs(address, _commitMessageText.Text));
			}
		}

		private void UpdateName()
		{
			if (_repository.GetUserIdInUse() != _userName.Text.Trim() && _userName.Text.Trim().Length>0)
			{
				_repository.SetUserNameInIni(_userName.Text.Trim(), new NullProgress());
			}
		}

		private void _useInternetButton_Click(object sender, EventArgs e)
		{
			if (RepositoryChosen != null)
			{
				UpdateName();
				var address = _repository.GetDefaultNetworkAddress<HttpRepositoryPath>();
				RepositoryChosen.Invoke(this, new SyncStartArgs(address, _commitMessageText.Text));
			}
		}

		private void _useSharedFolderButton_Click(object sender, EventArgs e)
		{
			if (RepositoryChosen != null)
			{
				UpdateName();
				var address = _repository.GetDefaultNetworkAddress<DirectoryRepositorySource>();
				RepositoryChosen.Invoke(this, new SyncStartArgs(address, _commitMessageText.Text));
			}
		}

		private void _internetStatusLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using(var dlg = new ServerSettingsDialog(_repository.PathToRepo))
			{
				dlg.ShowDialog();
			}

			UpdateInternetSituation();
		}

		private void _sharedFolderStatusLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if(DialogResult.Cancel ==
				MessageBox.Show(
				"Note, due to some limitations in the underlying system (Mercurial), connecting to a shared folder hosted by a Windows computer is not recommended. If the server is Linux, it's OK.",
				"Warning", MessageBoxButtons.OKCancel))
			{
				return;
			}
			using (var dlg =  new System.Windows.Forms.FolderBrowserDialog())
			{
				dlg.ShowNewFolderButton = true;
				dlg.Description = "Choose the folder containing the project with which you want to synchronize.";
				if (DialogResult.OK != dlg.ShowDialog())
					return;
				_model.SetNewSharedNetworkAddress(_repository, dlg.SelectedPath);
			}

			UpdateLocalNetworkSituation();
		}

		private void _internetDiagnosticsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Palaso.Reporting.ErrorReport.NotifyUserOfProblem(_connectionDiagnostics,
				"Internet", (string)_internetDiagnosticsLink.Tag);
		}

		private void _sharedNetworkDiagnosticsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Palaso.Reporting.ErrorReport.NotifyUserOfProblem(_connectionDiagnostics,
				"Shared Network Folder", (string)_sharedNetworkDiagnosticsLink.Tag);
		}
	}

	public class SyncStartArgs : EventArgs
	{
		public SyncStartArgs(RepositoryAddress address, string commitMessage)
		{
			Address = address;
			CommitMessage = commitMessage;
		}
		public RepositoryAddress Address;
		public string CommitMessage;
	}

	internal class InternetStateWorker
	{
		internal volatile bool _shouldQuit;
		private Action _action;

		internal InternetStateWorker(Action action)
		{
			_action = action;
		}

		internal void RequestStop()
		{
			_shouldQuit = true;
		}

		internal void DoWork()
		{
			while (!_shouldQuit)
			{
				_action();
				Thread.Sleep(2000);
				Console.WriteLine("worker thread: working...");
			}
			Console.WriteLine("worker thread: terminating gracefully.");
		}
	}
}
