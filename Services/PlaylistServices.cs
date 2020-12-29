using System.IO;
using MixtapeGui.Models;

namespace MixtapeGui.Services
{
    public class PlaylistService
    {
        public static void Import(Project project, string m3ufilename)
        {
            string playlistContent = File.ReadAllText(m3ufilename);
            string[] lines = playlistContent.Split('\r', System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith('#'))
                {
                    continue;
                }
                project.AddMusicFile(new MusicFile(project, line));
            }
        }
    }
}
