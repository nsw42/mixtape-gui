using System.IO;

namespace PlaylistEditor.Models
{
    public class MusicFile
    {
        public string SourceFile { get; set; }
        public string CachedWavFile { get; set; }

        public string Title { get; }

        public MusicFile(string sourceFile)
        {
            SourceFile = sourceFile;

            Title = Path.GetFileNameWithoutExtension(sourceFile); // TODO: Get the actual song title from the id3

            // TODO: Prepare cached wav file
        }
    }
}
