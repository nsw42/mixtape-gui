using System.Collections.Generic;
using System.IO;

namespace MixtapeGui.Models
{
    public class Project
    {
        public string ProjectFilename { get; set; }

        public List<MusicFile> MusicFiles { get; set; }

        public Project()
        {
            MusicFiles = new List<MusicFile>();
        }

        public Project(string filename) : this()
        {
            ProjectFilename = filename;
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
