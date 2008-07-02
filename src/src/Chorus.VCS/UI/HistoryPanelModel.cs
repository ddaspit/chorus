using System;
using System.Collections.Generic;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI
{
	internal class HistoryPanelModel
	{
		private readonly ProjectFolderConfiguration _project;
		private IProgress _progress;

		public HistoryPanelModel(ProjectFolderConfiguration project, IProgress progress)
		{
			_project = project;
			_progress = progress;
			throw new NotImplementedException();
		}

		public List<RevisionDescriptor> GetHistoryItems()
		{
			RepositoryManager manager = RepositoryManager.FromRootOrChildFolder(_project);
			return manager.GetHistoryItems(_progress);
		}
	}
}