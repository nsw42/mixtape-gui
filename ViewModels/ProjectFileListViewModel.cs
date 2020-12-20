using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using PlaylistEditor.Models;

namespace PlaylistEditor.ViewModels
{
    public class ProjectFileListViewModel : ViewModelBase
    {
        public ProjectFileListViewModel(Project project)
        {
            _project = project;
            PlacedItems = new ObservableCollection<MusicFile>(project.MusicFiles.FindAll((musicFile) => (musicFile.CanvasPosition != null)));
            UnplacedItems = new ObservableCollection<MusicFile>(project.MusicFiles.FindAll((musicFile) => (musicFile.CanvasPosition == null)));
        }

        private Project _project;

        public ObservableCollection<MusicFile> PlacedItems { get; }
        public ObservableCollection<MusicFile> UnplacedItems { get; }

        public void AddFile(string filename)
        {
            if (filename.EndsWith(".mp3")) {
                MusicFile mf = new MusicFile(_project, filename);
                _project.AddMusicFile(mf);
                UnplacedItems.Add(mf);
                ModelIO.SaveProject(_project);
            }
        }

        public void PlaceFile(MusicFile musicFile, Point p)
        {
            musicFile.CanvasPosition = p;
            UnplacedItems.Remove(musicFile);
            PlacedItems.Add(musicFile);
        }
    }
}
