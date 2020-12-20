using System.Collections.Generic;
using System.Collections.ObjectModel;
using PlaylistEditor.Models;

namespace PlaylistEditor.ViewModels
{
    public class ProjectViewModel : ViewModelBase
    {
        private Project _project;

        public ProjectViewModel(Project project)
        {
            _project = project;
            FileList = new ProjectFileListViewModel(project);
        }

        public ProjectFileListViewModel FileList { get; }

        public string Title { get {
            return _project.ProjectDirectory;
        } }

    }
}
