using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;  // TODO - remove me
using PlaylistEditor.Models;
using PlaylistEditor.ViewModels;

namespace PlaylistEditor.Views
{
    public class ProjectFileListView : UserControl
    {
        public ProjectFileListView()
        {
            this.InitializeComponent();

            // Set up drag and drop
            AddHandler(DragDrop.DragOverEvent, DragOver);
            AddHandler(DragDrop.DropEvent, DragEnd);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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

        void DragEnd(object sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.FileNames) &&
                (DataContext is ProjectViewModel viewModel))
            {
                foreach (string fn in e.Data.GetFileNames()) {
                    viewModel.FileList.AddFile(fn);
                }
            }
        }
    }
}
