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

        public void OnAlignHorizontally(object sender, RoutedEventArgs args)
        {
            if (DataContext is ProjectViewModel viewModel)
            {
                viewModel.AlignSelectedItemsHorizontally();
            }
            var canvasParent = this.FindControl<UserControl>("PlaylistCanvasView");
            canvasParent.InvalidateVisual();
        }
    }
}
