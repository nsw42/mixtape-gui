using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MixtapeGui.Services;
using MixtapeGui.ViewModels;

namespace MixtapeGui.Views
{
    public class ProjectWindow : Window
    {
        public ProjectWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void OnSaveClicked(object sender, EventArgs args)
        {
            if (DataContext is ProjectViewModel viewModel)
            {
                IOService.SaveProject(viewModel.Project);
            }
        }

        public void OnCloseClicked(object sender, EventArgs args)
        {
            Close();
        }

        public void OnDeleteItem(object sender, EventArgs args)
        {
            if (DataContext is ProjectViewModel viewModel)
            {
                foreach (var mf in viewModel.SelectedItems)
                {
                    viewModel.RemoveFile(mf);
                }
            }
        }

        public void OnLeftAlign()
        {
            if (DataContext is ProjectViewModel viewModel)
            {
                viewModel.LeftAlignSelectedItems();
            }
            var canvasParent = this.FindControl<UserControl>("PlaylistCanvasView");
            canvasParent.InvalidateVisual();
        }

        public void OnLeftAlign(object sender, RoutedEventArgs args)
        {
            OnLeftAlign();
        }

        public void OnLeftAlign(object sender, EventArgs args)
        {
            OnLeftAlign();
        }

        public void OnRightAlign()
        {
            if (DataContext is ProjectViewModel viewModel)
            {
                viewModel.RightAlignSelectedItems();
            }
            var canvasParent = this.FindControl<UserControl>("PlaylistCanvasView");
            canvasParent.InvalidateVisual();
        }

        public void OnRightAlign(object sender, RoutedEventArgs args)
        {
            OnRightAlign();
        }

        public void OnRightAlign(object sender, EventArgs args)
        {
            OnRightAlign();
        }
    }
}
