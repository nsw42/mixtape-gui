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
        private MusicFile MouseOverMusicFile = null;
        private Size DrawSize = new Size(200, 50);
        private Pen BlackPen = new Pen(Colors.Black.ToUint32());
        private Pen HighlightPen = new Pen(Colors.Yellow.ToUint32(), thickness: 3);
        private MusicFile MovingMusicFile = null;
        private Point MusicFileCanvasOffsetFromMousePointer;

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

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            e.Pointer.Capture(this);
            e.Handled = true;
            if (MouseOverMusicFile != null) {
                MovingMusicFile = MouseOverMusicFile;
                MusicFileCanvasOffsetFromMousePointer = e.GetPosition(this) - MouseOverMusicFile.CanvasPosition.Value;
            }
            base.OnPointerPressed(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            e.Handled = true;
            MovingMusicFile = null;
            base.OnPointerReleased(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            if (MovingMusicFile != null)
            {
                MovingMusicFile.CanvasPosition = mousePos - MusicFileCanvasOffsetFromMousePointer;
                InvalidateVisual();
            }
            else
            {
                MusicFile OldMouseOverMusicFile = MouseOverMusicFile;
                MouseOverMusicFile = null;
                if (DataContext is ProjectViewModel viewModel)
                {
                    foreach (var mf in viewModel.FileList.PlacedItems)
                    {
                        if ((mf.CanvasPosition.Value.X <= mousePos.X) &&
                            (mousePos.X <= mf.CanvasPosition.Value.X + DrawSize.Width) &&
                            (mf.CanvasPosition.Value.Y <= mousePos.Y) &&
                            (mousePos.Y <= mf.CanvasPosition.Value.Y + DrawSize.Height))
                        {
                            MouseOverMusicFile = mf;
                            break;
                        }
                    }
                }
                if (MouseOverMusicFile != OldMouseOverMusicFile)
                {
                    InvalidateVisual();
                }
            }
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
                Point textOffset = new Point(0, 1);
                foreach (var mf in viewModel.FileList.PlacedItems)
                {
                    Rect r = new Rect(mf.CanvasPosition.Value, DrawSize);
                    context.FillRectangle(Brushes.AliceBlue, r);
                    if (mf == MouseOverMusicFile)
                        context.DrawRectangle(HighlightPen, r);
                    context.DrawRectangle(BlackPen, r);

                    FormattedText t = new FormattedText {
                        Constraint = DrawSize,
                        TextAlignment = TextAlignment.Center,
                        Typeface = Typeface.Default,
                        Text = mf.Title,
                    };
                    context.DrawText(Brushes.Black, r.TopLeft + textOffset, t);
                }
            }
        }
    }
}
