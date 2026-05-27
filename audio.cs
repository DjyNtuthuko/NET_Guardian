using System;
using System.IO;
using System.Reflection;
using System.Windows.Media;

namespace NET_Guardian
{
    public class audio
    {
        private MediaPlayer player;

        public void PlayAudioGreeting(Action onAudioComplete)
        {
            try
            {
                // Write the embedded wav to a temp file so MediaPlayer can read it
                string tempFile = Path.Combine(Path.GetTempPath(), "NetGuardianAudio.wav");

                Stream resourceStream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("NET_Guardian.NetGuardianAudio.wav");
                if (resourceStream == null)
                {
                    Console.WriteLine("Could not find the embedded audio file.");
                    return;
                }
                using (FileStream output = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                {
                    resourceStream.CopyTo(output);
                }
                resourceStream.Dispose();

                // Set up the player and play the audio
                player = new MediaPlayer();
                player.Open(new Uri(tempFile, UriKind.Absolute));
                player.MediaEnded += (s, e) => onAudioComplete?.Invoke();
                player.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Audio playback error: " + ex.Message);
            }
        }
    }
}