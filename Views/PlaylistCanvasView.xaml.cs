using System;
using System.Collections.Generic;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ReactiveUI;
using PlaylistEditor.Models;
using PlaylistEditor.Services;
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
            InwardConnectionPoint,
            OutwardConnectionPoint,
            PlayTransition,
        };

        private enum CurrentMouseDownActionEnum
        {
            None,
            MovingFile,
            DrawingConnection
        };

        // properties to track state
        CurrentMouseDownActionEnum CurrentMouseDownAction = CurrentMouseDownActionEnum.None;

        // properties related to what the mouse is over
        // updated in OnPointerMoved if CurrentMouseDownAction == None
        private MusicFile MouseOverMusicFile = null;
        private MouseOverSymbol MouseOverElement = MouseOverSymbol.None;

        // properties used when CurrentMouseDownAction == MovingFile
        private MusicFile MovingMusicFile = null;
        private Point MusicFileCanvasOffsetFromMousePointer;

        // properties used when CurrentMouseDownAction == DrawingConnection
        private MusicFile DrawingConnectionFromMusicFile = null;
        private Point DrawingConnectionCurrentMousePos;
        private MusicFile DrawingConnectionToMusicFile = null;

        // properties related to drawing
        private readonly Size DrawSize = new Size(200, 50);
        private readonly Pen BlackPen = new Pen(Colors.Black.ToUint32());
        private readonly Pen HighlightPen = new Pen(Colors.Yellow.ToUint32(), thickness: 3);
        private readonly Point TextOffset = new Point(0, 2);
        private const int PlayWidth = 20;
        private const int PlayHeight = 20;
        private Geometry PlaySymbol;
        private const int ConnectionPointSize = 12;
        private Geometry ConnectionPointSymbol;
        private ScrollViewer ScrollWidget;
        private TranslateTransform CanvasToScreenTransform;
        private TranslateTransform ScreenToCanvasTransform;

        public PlaylistCanvasView()
        {
            InitializeComponent();

            // Set up geometries for rendering
            Point p0 = new Point(0, 0);
            Point p1 = new Point(0, PlayHeight);
            Point p2 = new Point(PlayWidth, PlayHeight / 2);
            PlaySymbol = new PolylineGeometry(new List<Point>{p0, p1, p2}, true);

            ConnectionPointSymbol = new EllipseGeometry(new Rect(0, 0, ConnectionPointSize, ConnectionPointSize));

            ReactiveCommand<Unit, Unit> recalculateScrollTransforms = ReactiveCommand.Create(
                () => RecalculateScrollTransforms(),
                outputScheduler: RxApp.MainThreadScheduler
            );

            // Set up transform objects for scrolling
            ScrollWidget = this.FindControl<ScrollViewer>("ScrollWidget");
            ScrollWidget.GetObservable(ScrollViewer.OffsetProperty)
                .Subscribe(offset => {
                    System.Diagnostics.Trace.WriteLine("Scroll changed");
                    recalculateScrollTransforms.Execute().Subscribe();
                });

            this.GetObservable(PlaylistCanvasView.DataContextProperty)
                .Subscribe(context => {
                    if (context is ProjectViewModel vm)
                    {
                        vm.PropertyChanged += (sender, args) => {
                            if (args.PropertyName == "CanvasX0" || args.PropertyName == "CanvasY0") {
                                System.Diagnostics.Trace.WriteLine("VM Property changed");
                                recalculateScrollTransforms.Execute().Subscribe();
                            }
                        };
                    }
                });

            RecalculateScrollTransforms();

            // Set up drag and drop
            AddHandler(DragDrop.DragOverEvent, DragOver);
            AddHandler(DragDrop.DropEvent, DragEnd);
        }

        private void RecalculateScrollTransforms()
        {
            var viewModel = DataContext as ProjectViewModel;
            var offset = ScrollWidget.Offset;
            System.Diagnostics.Trace.WriteLine("Recalculating scroll transforms");
            CanvasToScreenTransform = new TranslateTransform(-offset.X - (viewModel?.CanvasX0 ?? 0) + ProjectViewModel.ScrollMargin,
                                                                -offset.Y - (viewModel?.CanvasY0 ?? 0) + ProjectViewModel.ScrollMargin);
            ScreenToCanvasTransform = new TranslateTransform(-CanvasToScreenTransform.X, -CanvasToScreenTransform.Y);
            // System.Diagnostics.Trace.WriteLine($"CanvasToScreenTransform: {CanvasToScreenTransform.X}, {CanvasToScreenTransform.Y}");
            // System.Diagnostics.Trace.WriteLine($"ScreenToCanvasTransform: {ScreenToCanvasTransform.X}, {ScreenToCanvasTransform.Y}");
            InvalidateVisual();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            e.Pointer.Capture(this);
            e.Handled = true;
            CurrentMouseDownAction = CurrentMouseDownActionEnum.None;
            if (MouseOverMusicFile != null) {
                switch (MouseOverElement) {
                    case MouseOverSymbol.None:
                        break;
                    case MouseOverSymbol.MoveFile:
                        CurrentMouseDownAction = CurrentMouseDownActionEnum.MovingFile;
                        MovingMusicFile = MouseOverMusicFile;
                        var mousePos = e.GetPosition(this);
                        // System.Diagnostics.Trace.WriteLine($"mouse down: screen: {mousePos.X}, {mousePos.Y}");
                        mousePos = mousePos.Transform(ScreenToCanvasTransform.Value);
                        // System.Diagnostics.Trace.WriteLine($"            canvas: {mousePos.X}, {mousePos.Y}");
                        MusicFileCanvasOffsetFromMousePointer = mousePos - new Point(MouseOverMusicFile.CanvasX, MouseOverMusicFile.CanvasY);
                        break;
                    case MouseOverSymbol.PlayIntro:
                        AudioService.StartPlayingFile(MouseOverMusicFile.CachedIntroWavFile);
                        break;
                    case MouseOverSymbol.PlayOutro:
                        AudioService.StartPlayingFile(MouseOverMusicFile.CachedOutroWavFile);
                        break;
                    case MouseOverSymbol.InwardConnectionPoint:
                        // TODO
                        break;
                    case MouseOverSymbol.OutwardConnectionPoint:
                        CurrentMouseDownAction = CurrentMouseDownActionEnum.DrawingConnection;
                        DrawingConnectionFromMusicFile = MouseOverMusicFile;
                        break;
                    case MouseOverSymbol.PlayTransition:
                        string[] files = new string[] { MouseOverMusicFile.CachedOutroWavFile, MouseOverMusicFile.NextMusicFile.CachedIntroWavFile };
                        AudioService.StartPlayingFileList(files);
                        break;
                }
            }
            base.OnPointerPressed(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            e.Handled = true;

            if (CurrentMouseDownAction == CurrentMouseDownActionEnum.DrawingConnection &&
                DrawingConnectionFromMusicFile != null)
            {
                DrawingConnectionFromMusicFile.NextMusicFile = DrawingConnectionToMusicFile;  // Note that this can be null
            }

            CurrentMouseDownAction = CurrentMouseDownActionEnum.None;
            MovingMusicFile = DrawingConnectionFromMusicFile = DrawingConnectionToMusicFile = null;
            OnPointerMoved(e); // recalculate (and hence highlight) the file under the pointer
            base.OnPointerReleased(e);
        }

        MouseOverSymbol GetMouseOverSymbol(MusicFile musicFile, Point mousePos)
        {
            SetPlaySymbolTransformForIntro(musicFile);
            if (PlaySymbol.FillContains(mousePos)) {
                return MouseOverSymbol.PlayIntro;
            }

            SetPlaySymbolTransformForOutro(musicFile);
            if (PlaySymbol.FillContains(mousePos)) {
                return MouseOverSymbol.PlayOutro;
            }

            SetConnectionPointSymbolTransformForInward(musicFile);
            if (ConnectionPointSymbol.FillContains(mousePos)) {
                return MouseOverSymbol.InwardConnectionPoint;
            }

            SetConnectionPointSymbolTransformForOutward(musicFile);
            if (ConnectionPointSymbol.FillContains(mousePos)) {
                return MouseOverSymbol.OutwardConnectionPoint;
            }

            if ((musicFile.CanvasX <= mousePos.X) && (mousePos.X <= musicFile.CanvasX + DrawSize.Width) &&
                (musicFile.CanvasY <= mousePos.Y) && (mousePos.Y <= musicFile.CanvasY + DrawSize.Height))
            {
                return MouseOverSymbol.MoveFile;
            }

            return MouseOverSymbol.None;
        }

        private MusicFile GetMusicFileUnderMousePointer(Point mousePos)
        {
            if (DataContext is ProjectViewModel viewModel)
            {
                foreach (var mf in viewModel.PlacedItems)
                {
                    // Start off testing if we're *near* the MusicFile's rectangle
                    // GetMouseOverSymbol repeats this test without the fudge factor
                    // for the connection point symbol
                    if ((mf.CanvasX - ConnectionPointSize / 2 <= mousePos.X) &&
                        (mousePos.X <= mf.CanvasX + DrawSize.Width + ConnectionPointSize / 2) &&
                        (mf.CanvasY - ConnectionPointSize / 2 <= mousePos.Y) &&
                        (mousePos.Y <= mf.CanvasY + DrawSize.Height + ConnectionPointSize / 2))
                    {
                        return mf;
                    }
                }
            }
            return null;
        }

        private MusicFile GetTransitionUnderMousePointer(Point mousePos)
        {
            if (DataContext is ProjectViewModel viewModel)
            {
                foreach (var mf in viewModel.PlacedItems)
                {
                    var next = mf.NextMusicFile;
                    if (next == null)
                        continue;
                    SetPlaySymbolTransformForConnection(mf, next);
                    if (PlaySymbol.FillContains(mousePos))
                        return mf;
                }
            }
            return null;
        }


        protected override void OnPointerMoved(PointerEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            mousePos = mousePos.Transform(ScreenToCanvasTransform.Value);
            ProjectViewModel viewModel = DataContext as ProjectViewModel;
            if (viewModel == null)
            {
                return;
            }
            switch (CurrentMouseDownAction)
            {
                case CurrentMouseDownActionEnum.MovingFile:
                    viewModel.PlaceFile(MovingMusicFile, mousePos - MusicFileCanvasOffsetFromMousePointer);
                    InvalidateVisual();
                    break;

                case CurrentMouseDownActionEnum.DrawingConnection:
                    DrawingConnectionToMusicFile = GetMusicFileUnderMousePointer(mousePos);
                    if (DrawingConnectionToMusicFile == DrawingConnectionFromMusicFile)
                    {
                        DrawingConnectionToMusicFile = null;
                    }
                    if (DrawingConnectionToMusicFile == null) {
                        DrawingConnectionCurrentMousePos = mousePos;
                    } else {
                        // snap to the connection point for this music file
                        DrawingConnectionCurrentMousePos = DrawingConnectionToMusicFile.CanvasPosition;  // because the inward connector is top-left corner
                    }
                    InvalidateVisual();
                    break;

                case CurrentMouseDownActionEnum.None:
                    MusicFile OldMouseOverMusicFile = MouseOverMusicFile;
                    MouseOverSymbol OldMouseOverElement = MouseOverElement;
                    MouseOverMusicFile = GetMusicFileUnderMousePointer(mousePos);
                    if (MouseOverMusicFile == null)
                    {
                        MouseOverMusicFile = GetTransitionUnderMousePointer(mousePos);
                        MouseOverElement = (MouseOverMusicFile != null) ? MouseOverSymbol.PlayTransition : MouseOverSymbol.None;
                    }
                    else
                    {
                        MouseOverElement = GetMouseOverSymbol(MouseOverMusicFile, mousePos);
                        if (MouseOverElement == MouseOverSymbol.None)
                        {
                            MouseOverMusicFile = null;  // We were close to the box but not inside it
                        }
                    }
                    viewModel.SelectedItem = MouseOverMusicFile;
                    if (MouseOverMusicFile != OldMouseOverMusicFile || MouseOverElement != OldMouseOverElement)
                    {
                        InvalidateVisual();
                    }
                    break;
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
                    var mousePos = e.GetPosition(this);
                    // System.Diagnostics.Trace.WriteLine($"DragEnd: {mf.Title} to screenpos {mousePos}");
                    mousePos = mousePos.Transform(ScreenToCanvasTransform.Value);
                    // System.Diagnostics.Trace.WriteLine($"  canvas position {mousePos}");
                    viewModel.PlaceFile(mf, mousePos);
                    InvalidateVisual();
                }
            }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var viewModel = DataContext as ProjectViewModel;

            if (!(VisualRoot is Window w) || (viewModel == null))
            {
                return;
            }

            var transform = context.PushPreTransform(CanvasToScreenTransform.Value);

            // Firstly, draw all of the files
            foreach (var mf in viewModel.PlacedItems)
            {
                Rect r = new Rect(mf.CanvasPosition, DrawSize);
                context.FillRectangle(Brushes.AliceBlue, r);
                if (mf == MouseOverMusicFile && MouseOverElement != MouseOverSymbol.PlayTransition)
                    context.DrawRectangle(HighlightPen, r);
                context.DrawRectangle(BlackPen, r);

                FormattedText t = new FormattedText {
                    Constraint = DrawSize,
                    TextAlignment = TextAlignment.Center,
                    Typeface = Typeface.Default,
                    Text = mf.Title,
                };
                context.DrawText(Brushes.Black, r.TopLeft + TextOffset, t);

                SetPlaySymbolTransformForIntro(mf);
                context.DrawGeometry(Brushes.Black,
                                        (mf == MouseOverMusicFile && MouseOverElement == MouseOverSymbol.PlayIntro) ? HighlightPen : BlackPen,
                                        PlaySymbol);

                SetPlaySymbolTransformForOutro(mf);
                context.DrawGeometry(Brushes.Black,
                                        (mf == MouseOverMusicFile && MouseOverElement == MouseOverSymbol.PlayOutro) ? HighlightPen : BlackPen,
                                        PlaySymbol);

                SetConnectionPointSymbolTransformForInward(mf);
                context.DrawGeometry(Brushes.MediumTurquoise,
                                        (mf == MouseOverMusicFile && MouseOverElement == MouseOverSymbol.InwardConnectionPoint) ? HighlightPen : BlackPen,
                                        ConnectionPointSymbol);

                SetConnectionPointSymbolTransformForOutward(mf);
                context.DrawGeometry(Brushes.MediumTurquoise,
                                        (mf == MouseOverMusicFile && MouseOverElement == MouseOverSymbol.OutwardConnectionPoint) ? HighlightPen : BlackPen,
                                        ConnectionPointSymbol);
            }

            // Then overlay all of the connections
            foreach (var mf in viewModel.PlacedItems)
            {
                // Don't draw an existing connection if we're in the process of drawing
                // a replacement
                if (mf != DrawingConnectionFromMusicFile && mf.NextMusicFile != null)
                {
                    var outwardConnectionPoint = new Point(mf.CanvasX + DrawSize.Width,
                                                            mf.CanvasY + DrawSize.Height);
                    var next = mf.NextMusicFile;
                    var inwardConnectionPoint = next.CanvasPosition;
                    bool isHighlighted = (mf == MouseOverMusicFile && MouseOverElement == MouseOverSymbol.PlayTransition);
                    if (isHighlighted)
                        context.DrawLine(HighlightPen, outwardConnectionPoint, inwardConnectionPoint);
                    context.DrawLine(BlackPen, outwardConnectionPoint, inwardConnectionPoint);

                    SetPlaySymbolTransformForConnection(mf, next);
                    context.DrawGeometry(Brushes.Black,
                                         isHighlighted ? HighlightPen : BlackPen,
                                         PlaySymbol);
                }
            }

            if (CurrentMouseDownAction == CurrentMouseDownActionEnum.DrawingConnection)
            {
                context.DrawLine(BlackPen,
                                 new Point(DrawingConnectionFromMusicFile.CanvasX + DrawSize.Width, DrawingConnectionFromMusicFile.CanvasY + DrawSize.Height),
                                 DrawingConnectionCurrentMousePos);
            }

            transform.Dispose();
        }

        private void SetPlaySymbolTransformForIntro(MusicFile musicFile)
        {
            TranslateTransform playIntroTransform = new TranslateTransform(musicFile.CanvasX + 5,
                                                                           musicFile.CanvasY + DrawSize.Height - 5 - PlayHeight);
            PlaySymbol.Transform = playIntroTransform;
        }

        private void SetPlaySymbolTransformForOutro(MusicFile musicFile)
        {
            TranslateTransform playOutroTransform = new TranslateTransform(musicFile.CanvasX + DrawSize.Width - 5 - PlayWidth,
                                                                           musicFile.CanvasY + DrawSize.Height - 5 - PlayHeight);
            PlaySymbol.Transform = playOutroTransform;
        }

        private void SetPlaySymbolTransformForConnection(MusicFile from, MusicFile to)
        {
            double x0 = from.CanvasX + DrawSize.Width;
            double x1 = to.CanvasX;
            double y0 = from.CanvasY + DrawSize.Height;
            double y1 = to.CanvasY;
            TranslateTransform playConnectionTransform = new TranslateTransform((x0+x1)/2 - PlayWidth/2,
                                                                                (y0+y1)/2 - PlayHeight/2);
            PlaySymbol.Transform = playConnectionTransform;
        }

        private void SetConnectionPointSymbolTransformForInward(MusicFile musicFile)
        {
            TranslateTransform inwardConnectionPointTransform = new TranslateTransform(musicFile.CanvasX - ConnectionPointSize / 2,
                                                                                       musicFile.CanvasY - ConnectionPointSize / 2);
            ConnectionPointSymbol.Transform = inwardConnectionPointTransform;
        }

        private void SetConnectionPointSymbolTransformForOutward(MusicFile musicFile)
        {
            TranslateTransform outwardConnectionPointTransform = new TranslateTransform(musicFile.CanvasX + DrawSize.Width - ConnectionPointSize / 2,
                                                                                        musicFile.CanvasY + DrawSize.Height - ConnectionPointSize / 2);
            ConnectionPointSymbol.Transform = outwardConnectionPointTransform;
        }
    }
}
