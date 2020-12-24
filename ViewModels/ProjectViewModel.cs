using System.Collections.Generic;
using System.Collections.ObjectModel;
using PlaylistEditor.Models;

namespace PlaylistEditor.ViewModels
{
    public class ProjectViewModel : ViewModelBase
    {
        public Project Project { get; set; }

        public ProjectViewModel(Project project)
        {
            Project = project;
            FileList = new ProjectFileListViewModel(project);
        }

        public ProjectFileListViewModel FileList { get; }

        public string Title { get {
            return Project.ProjectDirectory;
        } }
    }
}
