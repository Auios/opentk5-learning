using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

public static class Debug {
  public static void Init() {
    GLDebugProc DebugMessageDelegate = OnDebugMessage;
    GL.DebugMessageCallback(DebugMessageDelegate, nint.Zero);
    GL.Enable(EnableCap.DebugOutput);

    GL.Enable(EnableCap.DebugOutputSynchronous);
  }

  public static void OnDebugMessage(
        DebugSource source,
        DebugType type,
        uint id,
        DebugSeverity severity,
        int length,
        nint pmessage,
        nint userParam) {
    string message = Marshal.PtrToStringAnsi(pmessage, length);
    Console.WriteLine("[{0} source={1} type={2} id={3}] {4}", severity, source, type, id, message);
  }
}
