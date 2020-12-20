using System.Collections.Generic;
using System.IO;

namespace PlaylistEditor.Models
{
    public class Project
    {
        public string ProjectDirectory { get; set; }
        public List<MusicFile> MusicFiles { get; set; }

        public readonly string contentsFile;

        public Project(string projdir)
        {
            ProjectDirectory = projdir;
            MusicFiles = new List<MusicFile>();

            contentsFile = Path.Join(ProjectDirectory, "contents.json");
        }

        public void AddMusicFile(MusicFile musicFile) {
            MusicFiles.Add(musicFile);
        }
    }
}
