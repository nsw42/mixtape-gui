#define MACOS

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

using NAudio.Wave;
using NLayer.NAudioSupport;

using MixtapeGui.Models;

namespace MixtapeGui.Services
{
    public class ImportService
    {
        private static ConcurrentQueue<MusicFile> FilesToImport = new ConcurrentQueue<MusicFile>();
        private static Thread ImportThread = null;

        public static void ImportFile(MusicFile musicFile)
        {
            FilesToImport.Enqueue(musicFile);
            if (ImportThread == null)
            {
                ImportThread = new Thread(ImportThreadBody);
                ImportThread.IsBackground = true; // allow the OS to kill it
                ImportThread.Start();
            }
        }

        private static void ImportThreadBody()
        {
            while (true)
            {
                MusicFile musicFile;
                if (FilesToImport.TryDequeue(out musicFile))
                {
                    ImportOneFile(musicFile);
                }
                Thread.Sleep(1000);
            }
        }

        private static void ImportOneFile(MusicFile musicFile)
        {
#if MACOS
            using (var reader = new Mp3FileReader(musicFile.SourceFile, wf => new Mp3FrameDecompressor(wf)))
            {
                musicFile.DurationSeconds = reader.TotalTime.TotalSeconds;
            }

            if (!File.Exists(musicFile.CachedIntroWavFile) || !File.Exists(musicFile.CachedOutroWavFile)) {
                // NAudio doesn't seem to allow only partially converting a file.
                // So convert the whole file, extract the fragments, then delete the temporary file
                string tmpWav = Path.Join(musicFile.Project.TempDirectory, "tmp.wav");
                if (File.Exists(tmpWav)) {
                    File.Delete(tmpWav);
                }

                using (var reader = new Mp3FileReader(musicFile.SourceFile, wf => new Mp3FrameDecompressor(wf))) {
                    WaveFileWriter.CreateWaveFile(tmpWav, reader);
                }

                if (!File.Exists(musicFile.CachedIntroWavFile)) {
                    var reader = new AudioFileReader(tmpWav)
                                      .Take(TimeSpan.FromSeconds(MusicFile.IntroDurationSeconds));
                    WaveFileWriter.CreateWaveFile16(musicFile.CachedIntroWavFile, reader);
                }

                if (!File.Exists(musicFile.CachedOutroWavFile)) {
                    var reader = new AudioFileReader(tmpWav)
                                      .Skip(TimeSpan.FromSeconds(musicFile.DurationSeconds - MusicFile.OutroDurationSeconds))
                                      .Take(TimeSpan.FromSeconds(MusicFile.OutroDurationSeconds));
                    WaveFileWriter.CreateWaveFile16(musicFile.CachedOutroWavFile, reader);
                }

                File.Delete(tmpWav);
            }

            musicFile.CachedIntroWavFileExists = musicFile.CachedOutroWavFileExists = true;
#else
#error Another platform - decide whether to use the NLayer mp3 decoding, or use something faster
#endif
        }
    }
}
