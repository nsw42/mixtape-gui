using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
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

        async void StartDrag(object sender, Avalonia.Input.PointerPressedEventArgs eventArgs)
        {
            if (eventArgs.Source is TextBlock textBlock) {
                if (textBlock.DataContext is MusicFile musicFile) {
                    var dragData = new DataObject();
                    dragData.Set(DataFormats.Text, musicFile.Title);
                    dragData.Set("MusicFile", musicFile);
                    System.Diagnostics.Trace.WriteLine($"Start drag {musicFile.Title}");
                    var result = await DragDrop.DoDragDrop(eventArgs, dragData, DragDropEffects.Copy);
                    switch (result)
                    {
                        case DragDropEffects.Copy:
                        case DragDropEffects.Link:
                            // System.Diagnostics.Trace.WriteLine("Data was copied");
                            break;
                        case DragDropEffects.None:
                            // System.Diagnostics.Trace.WriteLine("The drag operation was cancelled");
                            break;
                    }
                }
            }
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
                !e.Data.Contains("MusicFile") &&
                DataContext is ProjectViewModel viewModel)
            {
                foreach (string fn in e.Data.GetFileNames()) {
                    viewModel.FileList.AddFile(fn);
                }
            }
        }
    }
}
