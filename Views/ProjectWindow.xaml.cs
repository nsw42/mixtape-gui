using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PlaylistEditor.Models;
using PlaylistEditor.ViewModels;

namespace PlaylistEditor.Views
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
                ModelIO.SaveProject(viewModel.Project);
            }
        }

        public void OnCloseClicked(object sender, EventArgs args)
        {
            Close();
        }
    }
}
