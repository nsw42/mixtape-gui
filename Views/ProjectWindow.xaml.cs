using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using playlist_editor.Models;

namespace playlist_editor.Views
{
    public class ProjectWindow : Window
    {
        private Models.Project _project;
        public Models.Project Project {
            get { return _project; }
            set { _project = value; Title = value.ProjectDirectory; }
        }

        public ProjectWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
