using System.Collections.Generic;

namespace playlist_editor.Models
{
    public class Project
    {
        public string ProjectDirectory { get; set; }
        public List<MusicFile> MusicFiles { get; set; }

        public Project(string projdir)
        {
            ProjectDirectory = projdir;
            MusicFiles = new List<MusicFile>();
        }
    }
}
