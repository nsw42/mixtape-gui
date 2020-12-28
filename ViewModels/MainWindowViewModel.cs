using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive;
using ReactiveUI;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PlaylistEditor.Models;
using PlaylistEditor.Services;
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
                        return OpenProject(result, create: true);
                    }
                }

                return null;
            });

            OpenExistingProjectCommand = ReactiveCommand.CreateFromTask(async () => {
                var dialog = new OpenFolderDialog();
                dialog.Title = "Open Existing Project";

                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var result = await dialog.ShowAsync(desktop.MainWindow);
                    if (result != null) {
                        return OpenProject(result, create: false);
                    }
                }

                return null;
            });

            ImportM3UPlaylistCommand = ReactiveCommand.CreateFromTask(async () => {
                var m3uDialog = new OpenFileDialog {
                    Title = "Select m3u file",
                    AllowMultiple = false,
                    Filters = new List<FileDialogFilter> {
                        new FileDialogFilter {
                            Name = "m3u files (.m3u)",
                            Extensions = new List<string> {"m3u"}
                        },
                        new FileDialogFilter {
                            Name = "All files",
                            Extensions = new List<string> {"*"}
                        }
                    }
                };

                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var m3ufiles = await m3uDialog.ShowAsync(desktop.MainWindow);
                    if (m3ufiles != null) {
                        // and now import
                        CreateNewProjectCommand.Execute().Subscribe(project => PlaylistService.Import(project, m3ufiles[0]));
                    }
                }
            });
        }

        private Project OpenProject(string projectDir, bool create)
        {
            Project project;
            if (create) {
                project = new Project(projectDir);
            } else {
                project = IOService.LoadProject(projectDir);
            }
            var window = new ProjectWindow
            {
                DataContext = new ProjectViewModel(project),
            };
            window.Show();

            return project;
        }

        public ReactiveCommand<Unit, Project> CreateNewProjectCommand { get; }
        public ReactiveCommand<Unit, Project> OpenExistingProjectCommand { get; }
        public ReactiveCommand<Unit, Unit> ImportM3UPlaylistCommand {get; }
    }
}
