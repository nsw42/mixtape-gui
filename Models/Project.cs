using System.Collections.Generic;
using System.IO;

namespace MixtapeGui.Models
{
    public class Project
    {
        public const string ProjectContentsFileLeaf = "contents.json";

        private string projectDirectory;
        public string ProjectDirectory { get => projectDirectory;
            set {
                projectDirectory = value;
                TempDirectory = Path.Join(projectDirectory, "tmp");
                if (!Directory.Exists(TempDirectory)) {
                    Directory.CreateDirectory(TempDirectory);
                }
            }
        }
        public string TempDirectory { get; private set; }
        public List<MusicFile> MusicFiles { get; set; }

        public string contentsFile {
            get => Path.Join(ProjectDirectory, ProjectContentsFileLeaf);
        }

        public Project()
        {
            MusicFiles = new List<MusicFile>();
        }

        public Project(string projdir) : this()
        {
            ProjectDirectory = projdir;
        }

        public void AddMusicFile(MusicFile musicFile)
        {
            MusicFiles.Add(musicFile);
        }

        public void Remove(MusicFile musicFile)
        {
            MusicFiles.Remove(musicFile);
        }
    }
}
