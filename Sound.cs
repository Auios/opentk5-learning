using OpenTK.Audio.OpenAL;

public sealed class Sound : IDisposable {
  private ALCDevice device;
  private ALCContext context;
  private int buffer;
  private int source;
  private bool disposed;

  public Sound(string wavPath) {
    this.device = OpenTK.Audio.OpenAL.ALC.ALC.OpenDevice((string)null);
    if (this.device == default)
      throw new InvalidOperationException("Failed to open OpenAL device.");

    this.context = OpenTK.Audio.OpenAL.ALC.ALC.CreateContext(this.device, []);
    if (this.context == default)
      throw new InvalidOperationException("Failed to create OpenAL context.");

    OpenTK.Audio.OpenAL.ALC.ALC.MakeContextCurrent(this.context);

    AL.GenBuffers(1, ref this.buffer);
    AL.GenSources(1, ref this.source);

    LoadWave(wavPath, out byte[] audioData, out Format format, out int sampleRate);
    AL.BufferData(this.buffer, format, audioData, audioData.Length, sampleRate);
    AL.Sourcei(this.source, SourcePNameI.Buffer, this.buffer);
    AL.Sourcei(this.source, SourcePNameI.Looping, 0);
    AL.Sourcef(this.source, SourcePNameF.Gain, 1f);
  }

  public void Play() {
    OpenTK.Audio.OpenAL.ALC.ALC.MakeContextCurrent(this.context);
    AL.SourceStop(this.source);
    AL.SourceRewind(this.source);
    AL.SourcePlay(this.source);
  }

  private static void LoadWave(string path, out byte[] data, out Format format, out int sampleRate) {
    using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    using BinaryReader reader = new(stream);

    if (new string(reader.ReadChars(4)) != "RIFF")
      throw new FormatException("Invalid WAV file header.");

    reader.ReadInt32(); // File size - 8
    if (new string(reader.ReadChars(4)) != "WAVE")
      throw new FormatException("Invalid WAV file type.");

    short channels = 0;
    short bitsPerSample = 0;
    sampleRate = 0;
    data = [];

    while (reader.BaseStream.Position < reader.BaseStream.Length) {
      string chunkId = new(reader.ReadChars(4));
      int chunkSize = reader.ReadInt32();

      switch (chunkId) {
        case "fmt ":
          short audioFormat = reader.ReadInt16();
          if (audioFormat != 1)
            throw new NotSupportedException("Only PCM WAV files are supported.");

          channels = reader.ReadInt16();
          sampleRate = reader.ReadInt32();
          reader.ReadInt32(); // byte rate
          reader.ReadInt16(); // block align
          bitsPerSample = reader.ReadInt16();
          if (chunkSize > 16)
            reader.ReadBytes(chunkSize - 16);
          break;

        case "data":
          data = reader.ReadBytes(chunkSize);
          break;

        default:
          reader.ReadBytes(chunkSize);
          break;
      }
    }

    if (data.Length == 0)
      throw new FormatException("WAV file is missing a data chunk.");

    format = (channels, bitsPerSample) switch {
      (1, 8) => (Format)0x1100,   // AL_FORMAT_MONO8
      (1, 16) => (Format)0x1101,  // AL_FORMAT_MONO16
      (2, 8) => (Format)0x1102,   // AL_FORMAT_STEREO8
      (2, 16) => (Format)0x1103,  // AL_FORMAT_STEREO16
      _ => throw new NotSupportedException($"Unsupported WAV format: {channels} channel(s), {bitsPerSample} bits.")
    };
  }

  public void Dispose() {
    if (this.disposed)
      return;

    this.disposed = true;
    OpenTK.Audio.OpenAL.ALC.ALC.MakeContextCurrent(this.context);
    AL.DeleteSources(1, ref this.source);
    AL.DeleteBuffers(1, ref this.buffer);
    OpenTK.Audio.OpenAL.ALC.ALC.MakeContextCurrent(default);
    OpenTK.Audio.OpenAL.ALC.ALC.DestroyContext(this.context);
    OpenTK.Audio.OpenAL.ALC.ALC.CloseDevice(this.device);
  }
}
