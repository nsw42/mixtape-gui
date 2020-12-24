#define MACOS

using System;
using System.IO;
using System.Text.Json.Serialization;
using Avalonia;
using NAudio.Wave;
using NLayer.NAudioSupport;
using PlaylistEditor.Models;

namespace PlaylistEditor.Models
{
    public class MusicFile
    {
        private const int IntroDurationSeconds = 15;
        private const int OutroDurationSeconds = 15;

        public string SourceFile { get; set; }
        public string CachedIntroWavFile { get; set; }
        public string CachedOutroWavFile { get; set; }

        [JsonInclude]
        public string Title { get; private set; }

        [JsonInclude]
        // using seconds allows for easy json serialization
        public double DurationSeconds { get; private set; }

        [JsonInclude]
        public double CanvasX { get; set; }

        [JsonInclude]
        public double CanvasY { get; set; }

        [JsonIgnore]
        public Point CanvasPosition {
            get { return new Point(CanvasX, CanvasY); }
            set { CanvasX = value.X; CanvasY=value.Y; }
        }

        public MusicFile NextMusicFile { get; set; }

        public MusicFile()
        {
            CanvasX = CanvasY = 0;
            NextMusicFile = null;
        }

        public MusicFile(Project project, string sourceFile) : this()
        {
            SourceFile = sourceFile;

            Title = Path.GetFileNameWithoutExtension(sourceFile); // TODO: Get the actual song title from the id3

            var tmpDir = Path.Join(project.ProjectDirectory, "tmp");
            if (!Directory.Exists(tmpDir)) {
                Directory.CreateDirectory(tmpDir);
            }

            CachedIntroWavFile = Path.Join(tmpDir, Title + "_intro.wav");
            CachedOutroWavFile = Path.Join(tmpDir, Title + "_outro.wav");

#if MACOS
            using (var reader = new Mp3FileReader(sourceFile, wf => new Mp3FrameDecompressor(wf)))
            {
                DurationSeconds = reader.TotalTime.TotalSeconds;
            }

            // TODO: This should be moved out into a separate Services class, or a separate thread, or something.
            // Right now, it blocks for a second or two when adding a file
            if (!File.Exists(CachedIntroWavFile) || !File.Exists(CachedOutroWavFile)) {
                // NAudio doesn't seem to allow only partially converting a file.
                // So convert the whole file, extract the fragments, then delete the temporary file
                string tmpWav = Path.Join(tmpDir, "tmp.wav");
                if (File.Exists(tmpWav)) {
                    File.Delete(tmpWav);
                }

                using (var reader = new Mp3FileReader(sourceFile, wf => new Mp3FrameDecompressor(wf))) {
                    WaveFileWriter.CreateWaveFile(tmpWav, reader);
                }

                if (!File.Exists(CachedIntroWavFile)) {
                    var reader = new AudioFileReader(tmpWav)
                                      .Take(TimeSpan.FromSeconds(IntroDurationSeconds));
                    WaveFileWriter.CreateWaveFile16(CachedIntroWavFile, reader);
                }

                if (!File.Exists(CachedOutroWavFile)) {
                    var reader = new AudioFileReader(tmpWav)
                                      .Skip(TimeSpan.FromSeconds(DurationSeconds - OutroDurationSeconds))
                                      .Take(TimeSpan.FromSeconds(OutroDurationSeconds));
                    WaveFileWriter.CreateWaveFile16(CachedOutroWavFile, reader);
                }

                File.Delete(tmpWav);
            }
#else
#error Another platform - decide whether to use the NLayer mp3 decoding, or use something faster
#endif
        }
    }
}
