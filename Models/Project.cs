using System.Collections.Generic;
using System.IO;

namespace MixtapeGui.Models
{
    public class Project
    {
        public const string ProjectContentsFileLeaf = "contents.json";

        public string ProjectDirectory { get; set; }
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
