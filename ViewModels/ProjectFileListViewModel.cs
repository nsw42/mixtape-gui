using System.Collections.Generic;
using System.Collections.ObjectModel;
using PlaylistEditor.Models;

namespace PlaylistEditor.ViewModels
{
    public class ProjectFileListViewModel : ViewModelBase
    {
        public ProjectFileListViewModel(Project project)
        {
            _project = project;
            Items = new ObservableCollection<MusicFile>(project.MusicFiles);
        }

        private Project _project;

        public ObservableCollection<MusicFile> Items { get; }

        public void AddFile(string filename)
        {
            if (filename.EndsWith(".mp3")) {
                MusicFile mf = new MusicFile(_project, filename);
                _project.AddMusicFile(mf);
                Items.Add(mf);
            }
        }
    }
}
