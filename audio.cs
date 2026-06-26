using System;
using System.IO;
using System.Reflection;
using System.Media;
using System.Threading.Tasks;

namespace NET_Guardian
{
    public class Audio
    {
        private SoundPlayer? soundPlayer;
        private string? tempFilePath;

        public void PlayAudioGreeting(Action onAudioComplete)
        {
            Console.WriteLine("PlayAudioGreeting called - starting audio playback on background thread...");
            // Run on a background thread to avoid blocking the UI
            Task.Run(() => PlayAudioAsync(onAudioComplete));
        }

        private void PlayAudioAsync(Action onAudioComplete)
        {
            try
            {
                Console.WriteLine("Audio playback thread started.");
                
                // Extract the embedded wav file to a temporary location
                tempFilePath = Path.Combine(Path.GetTempPath(), "NetGuardianAudioText.wav");
                Console.WriteLine($"Target temp file: {tempFilePath}");

                // Get the embedded resource stream
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "NET_Guardian.NetGuardianAudioText.wav";
                Stream? resourceStream = assembly.GetManifestResourceStream(resourceName);
                
                if (resourceStream == null)
                {
                    // Try alternative resource names
                    Console.WriteLine("Could not find embedded audio resource. Available resources:");
                    foreach (var res in assembly.GetManifestResourceNames())
                    {
                        Console.WriteLine($"  - {res}");
                    }
                    onAudioComplete?.Invoke();
                    return;
                }

                Console.WriteLine($"Found embedded resource: {resourceName} ({resourceStream.Length} bytes)");

                // Write the stream to a temporary file
                using (FileStream output = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                {
                    resourceStream.CopyTo(output);
                }
                resourceStream.Dispose();

                // Verify the file was written successfully
                if (!File.Exists(tempFilePath))
                {
                    Console.WriteLine("ERROR: Audio temp file was not created!");
                    onAudioComplete?.Invoke();
                    return;
                }

                var fileInfo = new FileInfo(tempFilePath);
                if (fileInfo.Length == 0)
                {
                    Console.WriteLine("ERROR: Audio temp file is empty!");
                    onAudioComplete?.Invoke();
                    return;
                }

                Console.WriteLine($"Audio temp file created successfully: {fileInfo.Length} bytes");

                // Create and configure the SoundPlayer
                soundPlayer = new SoundPlayer(tempFilePath);
                
                // Load the audio file into memory first for reliability
                Console.WriteLine("Loading audio file into memory...");
                soundPlayer.Load();
                Console.WriteLine("Audio file loaded. Starting playback...");

                // Play audio synchronously and wait for completion
                soundPlayer.PlaySync(); // Blocks until audio finishes
                Console.WriteLine("Audio playback completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Audio error: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                try
                {
                    CleanupAudio();
                    Console.WriteLine("Audio cleanup completed. Invoking callback...");
                    onAudioComplete?.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cleanup error: {ex.Message}");
                    onAudioComplete?.Invoke();
                }
            }
        }

        private void CleanupAudio()
        {
            try
            {
                soundPlayer?.Dispose();
                soundPlayer = null;

                // Clean up temp file with retry logic
                if (!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            File.Delete(tempFilePath);
                            Console.WriteLine("Temp audio file deleted.");
                            break;
                        }
                        catch
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cleanup exception: {ex.Message}");
            }
        }
    }
}