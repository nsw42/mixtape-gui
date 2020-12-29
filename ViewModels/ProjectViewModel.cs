using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using ReactiveUI;
using PlaylistEditor.Models;
using PlaylistEditor.Services;

namespace PlaylistEditor.ViewModels
{
    public class ProjectViewModel : ViewModelBase
    {
        public Project Project { get; set; }

        public string Title { get {
            return Project.ProjectDirectory;
        } }

        public const double ScrollMargin = 100;

        private double canvasX0;
        public double CanvasX0 { get => canvasX0;
                                 set => this.RaiseAndSetIfChanged(ref canvasX0, value); }

        private double canvasX1;
        public double CanvasX1 { get => canvasX1;
                                 set => this.RaiseAndSetIfChanged(ref canvasX1, value); }

        private double canvasY0;
        public double CanvasY0 { get => canvasY0;
                                 set => this.RaiseAndSetIfChanged(ref canvasY0, value); }

        private double canvasY1;
        public double CanvasY1 { get => canvasY1;
                                 set => this.RaiseAndSetIfChanged(ref canvasY1, value); }

        private ObservableAsPropertyHelper<double> visibleWidth;
        public double VisibleWidth => visibleWidth.Value;

        private ObservableAsPropertyHelper<double> visibleHeight;
        public double VisibleHeight => visibleHeight.Value;

        public ObservableCollection<MusicFile> PlacedItems { get; }
        public ObservableCollection<MusicFile> UnplacedItems { get; }

        public ProjectViewModel(Project project)
        {
            Project = project;

            visibleWidth = this
                .WhenAnyValue(vm => vm.CanvasX0, vm => vm.CanvasX1,
                              (x0, x1) => (ScrollMargin * 2 + x1 - x0))
                .ToProperty(this, x => x.VisibleWidth);

            visibleHeight = this
                .WhenAnyValue(vm => vm.CanvasY0, vm => vm.CanvasY1,
                              (y0, y1) => (ScrollMargin * 2 + y1 - y0))
                .ToProperty(this, x => x.VisibleHeight);

            PlacedItems = new ObservableCollection<MusicFile>(project.MusicFiles.FindAll((musicFile) => (musicFile.CanvasX != 0 && musicFile.CanvasY != 0)));
            UnplacedItems = new ObservableCollection<MusicFile>(project.MusicFiles.FindAll((musicFile) => (musicFile.CanvasX == 0 && musicFile.CanvasY == 0)));

            CanvasX0 = PlacedItems.Min(mf => mf.CanvasX);
            CanvasX1 = PlacedItems.Max(mf => mf.CanvasX);
            CanvasY0 = PlacedItems.Min(mf => mf.CanvasY);
            CanvasY1 = PlacedItems.Max(mf => mf.CanvasY);
        }

        public void AddFile(string filename)
        {
            if (filename.EndsWith(".mp3")) {
                MusicFile mf = new MusicFile(Project, filename);
                Project.AddMusicFile(mf);
                UnplacedItems.Add(mf);
                IOService.SaveProject(Project);
            }
        }

        public void PlaceFile(MusicFile musicFile, Point p)
        {
            bool alreadyPlaced = (musicFile.CanvasX != 0 || musicFile.CanvasY != 0);

            musicFile.CanvasPosition = p;
            CanvasX0 = Math.Min(CanvasX0, p.X);
            CanvasX1 = Math.Max(CanvasX1, p.X);
            CanvasY0 = Math.Min(CanvasY0, p.Y);
            CanvasY1 = Math.Max(CanvasY1, p.Y);

            if (!alreadyPlaced)
            {
                UnplacedItems.Remove(musicFile);
                PlacedItems.Add(musicFile);
            }
        }
    }
}
