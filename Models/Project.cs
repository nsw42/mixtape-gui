using System.Collections.Generic;
using System.IO;

namespace MixtapeGui.Models
{
    public class Project
    {
        public const string ProjectContentsFileLeaf = "contents.json";

        public string TempDirectory = Path.Join(Path.GetTempPath(), "mixtape");

        public string ProjectDirectory { get; set; }
        public List<MusicFile> MusicFiles { get; set; }

        public string contentsFile {
            get => Path.Join(ProjectDirectory, ProjectContentsFileLeaf);
        }

        public Project()
        {
            MusicFiles = new List<MusicFile>();
            if (!Directory.Exists(TempDirectory))
            {
                Directory.CreateDirectory(TempDirectory);
            }
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
