using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive;
using ReactiveUI;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MixtapeGui.Models;
using MixtapeGui.Services;
using MixtapeGui.Views;

namespace MixtapeGui.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        List<FileDialogFilter> FileDialogFilters() {
            return new List<FileDialogFilter> {
                        new FileDialogFilter {
                            Name = "Mixtape files (.mix)",
                            Extensions = new List<string> {"mix"}
                        },
                        new FileDialogFilter {
                            Name = "All files",
                            Extensions = new List<string> {"*"}
                        }
                    };
        }

        public MainWindowViewModel()
        {
            CreateNewProjectCommand = ReactiveCommand.Create(() => OpenProject("", create: true));

            OpenExistingProjectCommand = ReactiveCommand.CreateFromTask(async () => {
                OpenFileDialog dialog = new OpenFileDialog {
                    Title = "Open Existing Project",
                    AllowMultiple = false,
                    Filters = FileDialogFilters()
                };

                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var result = await dialog.ShowAsync(desktop.MainWindow);
                    if (result != null && result.Length > 0) {
                        return OpenProject(result[0], create: false);
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

        private Project OpenProject(string projectFilename, bool create)
        {
            Project project;
            if (create) {
                project = new Project(projectFilename);
            } else {
                project = IOService.LoadProject(projectFilename);
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
