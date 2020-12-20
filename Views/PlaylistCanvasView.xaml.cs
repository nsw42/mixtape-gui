using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using PlaylistEditor.Models;
using PlaylistEditor.ViewModels;

namespace PlaylistEditor.Views
{
    public class PlaylistCanvasView : UserControl
    {
        private Size DrawSize = new Size(200, 50);
        Pen BlackPen = new Pen(Colors.Black.ToUint32(), thickness: 3);

        public PlaylistCanvasView()
        {
            InitializeComponent();

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
            e.DragEffects = e.DragEffects & (DragDropEffects.Copy | DragDropEffects.Link);
            if (!e.Data.Contains("MusicFile")) {
                e.DragEffects = DragDropEffects.None;
            }
        }

        void DragEnd(object sender, DragEventArgs e)
        {
            if (e.Data.Contains("MusicFile"))
            {
                if (DataContext is ProjectViewModel viewModel)
                {
                    MusicFile mf = e.Data.Get("MusicFile") as MusicFile;
                    viewModel.FileList.PlaceFile(mf, e.GetPosition(this));
                    InvalidateVisual();
                }
            }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            if (!(VisualRoot is Window w))
            {
                return;
            }

            if (DataContext is ProjectViewModel viewModel)
            {
                foreach (var mf in viewModel.FileList.PlacedItems)
                {
                    Rect r = new Rect(mf.CanvasPosition.Value, DrawSize);
                    context.DrawRectangle(BlackPen, r);
                }
            }
        }
    }
}
