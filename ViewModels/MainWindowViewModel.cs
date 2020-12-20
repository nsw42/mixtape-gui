using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive;
using ReactiveUI;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PlaylistEditor.Models;
using PlaylistEditor.Views;

namespace PlaylistEditor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            CreateNewProjectCommand = ReactiveCommand.CreateFromTask(async () => {
                var dialog = new OpenFolderDialog();
                dialog.Title = "Create New Project";

                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var result = await dialog.ShowAsync(desktop.MainWindow);
                    if (result != null) {
                        OpenProject(result);
                    }
                }
            });

            OpenExistingProjectCommand = ReactiveCommand.CreateFromTask(async () => {
                var dialog = new OpenFolderDialog();
                dialog.Title = "Open Existing Project";

                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var result = await dialog.ShowAsync(desktop.MainWindow);
                    if (result != null) {
                        OpenProject(result);
                    }
                }
            });
        }

        private void OpenProject(string projectDir)
        {
            var project = new Project(projectDir);
            var window = new ProjectWindow
            {
                DataContext = new ProjectViewModel(project),
            };
            window.Show();
        }

        public ReactiveCommand<Unit, Unit> CreateNewProjectCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenExistingProjectCommand { get; }
    }
}
