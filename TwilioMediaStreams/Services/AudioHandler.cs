using System.IO;

using NAudio.Utils;
using NAudio.Wave;


namespace TwilioMediaStreams.Services
{
    public static class AudioHandler
    {
    public static void GenerateAudioStream(byte[] buffer, MemoryStream memoryStream)
    {
        // define the audio file type
        var waveFormat = WaveFormat.CreateMuLawFormat(8000, 1);

        // use WaveFileWriter to convert the audio file buffer and write it into a memory stream
        using (var waveFileWriter = new WaveFileWriter(new IgnoreDisposeStream(memoryStream), waveFormat))
        {
            waveFileWriter.Write(buffer, 0, buffer.Length);
            waveFileWriter.Flush();
        }
    }
    }
}
