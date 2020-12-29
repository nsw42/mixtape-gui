#define MAC
using System.Collections.Generic;
using System.Diagnostics;
using NAudio.Wave;
using System.Threading;

namespace MixtapeGui.Services
{
    public class AudioService
    {
        private static CancellationTokenSource CancellationToken = null;
        private static List<string> AudioFiles;

#if MAC
        private static void PlaybackThread(object obj)
        {
            CancellationToken token = (CancellationToken)obj;
            int playIndex = 0;
            while (playIndex < AudioFiles.Count && !token.IsCancellationRequested)
            {
                string audioFile = AudioFiles[playIndex++];
                var startInfo = new ProcessStartInfo("afplay", $"\"{audioFile}\"");
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                var process = Process.Start(startInfo);
                while (!process.HasExited && !token.IsCancellationRequested)
                {
                    Thread.Sleep(250);
                }
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
        }
#else
        private static void PlaybackThread(object obj)
        {
            CancellationToken token = (CancellationToken)obj;
            using(var audioFile = new AudioFileReader(AudioFile))
            using(var outputDevice = new DirectSoundOut())
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing && !token.IsCancellationRequested)
                {
                    Thread.Sleep(250);
                }
            }
        }
#endif

        public static void StartPlayingFile(string filename)
        {
            List<string> files = new List<string> { filename };
            StartPlayingFileList(files);
        }

        public static void StartPlayingFileList(List<string> files)
        {
            StopPlaying();
            CancellationToken = new CancellationTokenSource();
            AudioFiles = files;
            ThreadPool.QueueUserWorkItem(new WaitCallback(PlaybackThread), CancellationToken.Token);
        }

        public static void StopPlaying()
        {
            if (CancellationToken != null)
            {
                CancellationToken.Cancel();
                Thread.Sleep(500);
                CancellationToken.Dispose();
                CancellationToken = null;
            }
        }
    }
}
