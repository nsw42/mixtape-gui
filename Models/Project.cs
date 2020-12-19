using System.Collections.Generic;

namespace PlaylistEditor.Models
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
