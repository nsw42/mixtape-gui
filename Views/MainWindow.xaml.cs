using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MixtapeGui.ViewModels;

namespace MixtapeGui.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void OnOpenClicked(object sender, EventArgs args)
        {
            if (DataContext is MainWindowViewModel context)
            {
                context.OpenExistingProjectCommand.Execute();
            }
        }

        public void OnCloseClicked(object sender, EventArgs args)
        {
            Close();
        }
    }
}
