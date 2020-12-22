#define MAC
using System.Diagnostics;
using NAudio.Wave;
using System.Threading;

namespace PlaylistEditor
{
    public class AudioService
    {
        private static CancellationTokenSource CancellationToken = null;
        private static string AudioFile;

#if MAC
        private static void PlaybackThread(object obj)
        {
            CancellationToken token = (CancellationToken)obj;
            var startInfo = new ProcessStartInfo("afplay", $"\"{AudioFile}\"");
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
            if (CancellationToken != null)
            {
                CancellationToken.Cancel();
                Thread.Sleep(500);
                CancellationToken.Dispose();
            }
            CancellationToken = new CancellationTokenSource();
            AudioFile = filename;
            ThreadPool.QueueUserWorkItem(new WaitCallback(PlaybackThread), CancellationToken.Token);
        }
    }
}
