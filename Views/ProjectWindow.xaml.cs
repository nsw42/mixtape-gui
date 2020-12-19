using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PlaylistEditor.Models;

namespace PlaylistEditor.Views
{
    public class ProjectWindow : Window
    {
        private Models.Project _project;
        public Models.Project Project {
            get { return _project; }
            set { _project = value; Title = value.ProjectDirectory; }
        }

        TextBlock _FileDropArea;

        public ProjectWindow()
        {
            InitializeComponent();

            _FileDropArea = this.Find<TextBlock>("FileDropArea");

            // Set-up drag and drop
            AddHandler(DragDrop.DragOverEvent, DragOver);
            AddHandler(DragDrop.DropEvent, Drop);
        }

        void DragOver(object sender, DragEventArgs e)
        {
            // Only allow Copy or Link as Drop Operations.
            e.DragEffects = e.DragEffects & (DragDropEffects.Copy | DragDropEffects.Link);

            // Only allow if the dragged data contains filenames.
            if (!e.Data.Contains(DataFormats.FileNames)) {
                e.DragEffects = DragDropEffects.None;
            }
        }

        void Drop(object sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.FileNames)) {
                _FileDropArea.Text = string.Join(Environment.NewLine, e.Data.GetFileNames());
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
