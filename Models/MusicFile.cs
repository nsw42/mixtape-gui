
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Avalonia;
using MixtapeGui.Services;

namespace MixtapeGui.Models
{
    public class MusicFile
    {
        public const int IntroDurationSeconds = 15;
        public const int OutroDurationSeconds = 15;

        public string SourceFile { get; set; }
        public string CachedIntroWavFile { get; set; }
        public bool CachedIntroWavFileExists { get; set; }
        public string CachedOutroWavFile { get; set; }
        public bool CachedOutroWavFileExists { get; set; }

        public Project Project { get; set; }
        public string Title { get; set; }

        // using seconds allows for easy json serialization
        public double DurationSeconds { get; set; }

        public double CanvasX { get; set; }

        public double CanvasY { get; set; }

        public Point CanvasPosition {
            get { return new Point(CanvasX, CanvasY); }
            set { CanvasX = value.X; CanvasY=value.Y; }
        }

        public MusicFile PrevMusicFile { get; set; }
        public MusicFile NextMusicFile { get; set; }

        public MusicFile()
        {
            CanvasX = CanvasY = 0;
            PrevMusicFile = NextMusicFile = null;
        }

        public MusicFile(Project project, string sourceFile) : this()
        {
            Project = project;
            SourceFile = sourceFile;

            Title = Path.GetFileNameWithoutExtension(sourceFile); // TODO: Get the actual song title from the id3

            string hexdigest;
            using (SHA512 shaM = new SHA512Managed())
            {
                var hash = shaM.ComputeHash(Encoding.UTF8.GetBytes(sourceFile));
                hexdigest = BitConverter.ToString(hash).Replace("-","");
            }
            CachedIntroWavFile = Path.Join(App.TempDirectory, hexdigest + "_intro.wav");
            CachedOutroWavFile = Path.Join(App.TempDirectory, hexdigest + "_outro.wav");

            CachedIntroWavFileExists = CachedOutroWavFileExists = false;
            ImportService.ImportFile(this);
        }
    }
}
