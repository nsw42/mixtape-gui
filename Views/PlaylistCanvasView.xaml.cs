using System.Collections.Generic;
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
        private enum MouseOverSymbol
        {
            None,
            MoveFile,
            PlayIntro,
            PlayOutro,
        };

        private MusicFile MouseOverMusicFile = null;
        private MouseOverSymbol MouseOverElement = MouseOverSymbol.None;
        private Size DrawSize = new Size(200, 50);
        private Pen BlackPen = new Pen(Colors.Black.ToUint32());
        private Pen HighlightPen = new Pen(Colors.Yellow.ToUint32(), thickness: 3);
        private MusicFile MovingMusicFile = null;
        private Point MusicFileCanvasOffsetFromMousePointer;
        private const int PlayHeight = 20;
        private Geometry PlaySymbol;

        public PlaylistCanvasView()
        {
            InitializeComponent();

            Point p0 = new Point(0, 0);
            Point p1 = new Point(0, PlayHeight);
            Point p2 = new Point(PlayHeight, PlayHeight / 2);
            PlaySymbol = new PolylineGeometry(new List<Point>{p0, p1, p2}, true);

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
                switch (MouseOverElement) {
                    case MouseOverSymbol.None:
                        break;
                    case MouseOverSymbol.MoveFile:
                        MovingMusicFile = MouseOverMusicFile;
                        MusicFileCanvasOffsetFromMousePointer = e.GetPosition(this) - MouseOverMusicFile.CanvasPosition.Value;
                        break;
                    case MouseOverSymbol.PlayIntro:
                        AudioService.StartPlayingFile(MouseOverMusicFile.CachedIntroWavFile);
                        break;
                    case MouseOverSymbol.PlayOutro:
                        AudioService.StartPlayingFile(MouseOverMusicFile.CachedOutroWavFile);
                        break;
                }
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
                MouseOverSymbol OldMouseOverElement = MouseOverElement;
                MouseOverMusicFile = null;
                MouseOverElement = MouseOverSymbol.None;
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

                            SetPlaySymbolTransformForIntro(mf);
                            if (PlaySymbol.FillContains(mousePos)) {
                                MouseOverElement = MouseOverSymbol.PlayIntro;
                            } else {
                                SetPlaySymbolTransformForOutro(mf);
                                if (PlaySymbol.FillContains(mousePos)) {
                                    MouseOverElement = MouseOverSymbol.PlayOutro;
                                } else {
                                    MouseOverElement = MouseOverSymbol.MoveFile;
                                }
                            }
                            break;
                        }
                    }
                }
                if (MouseOverMusicFile != OldMouseOverMusicFile || MouseOverElement != OldMouseOverElement)
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

                    SetPlaySymbolTransformForIntro(mf);
                    context.DrawGeometry(Brushes.Black,
                                         (mf == MouseOverMusicFile && MouseOverElement == MouseOverSymbol.PlayIntro) ? HighlightPen : BlackPen,
                                         PlaySymbol);

                    SetPlaySymbolTransformForOutro(mf);
                    context.DrawGeometry(Brushes.Black,
                                         (mf == MouseOverMusicFile && MouseOverElement == MouseOverSymbol.PlayOutro) ? HighlightPen : BlackPen,
                                         PlaySymbol);
                }
            }
        }

        private void SetPlaySymbolTransformForIntro(MusicFile musicFile)
        {
            TranslateTransform playIntroTransform = new TranslateTransform(musicFile.CanvasPosition.Value.X + 5,
                                                                           musicFile.CanvasPosition.Value.Y + DrawSize.Height - 5 - PlayHeight);
            PlaySymbol.Transform = playIntroTransform;
        }

        private void SetPlaySymbolTransformForOutro(MusicFile musicFile)
        {
            TranslateTransform playOutroTransform = new TranslateTransform(musicFile.CanvasPosition.Value.X + DrawSize.Width - 5 - PlayHeight,
                                                                           musicFile.CanvasPosition.Value.Y + DrawSize.Height - 5 - PlayHeight);
            PlaySymbol.Transform = playOutroTransform;
        }
    }
}
