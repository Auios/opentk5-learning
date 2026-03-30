using OpenTK.Core.Utility;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;
using OpenTK.Windowing.Desktop;

public static class Window {
  public static WindowHandle handle = null!;
  public static OpenGLContextHandle context = null!;
  public static Vector2i size;

  public static void Init(int width, int height, string title) {
    size = new Vector2i(width, height);
    ToolkitOptions options = new() {
      Logger = new ConsoleLogger(),
    };
    Toolkit.Init(options);

    OpenGLGraphicsApiHints hints = new OpenGLGraphicsApiHints();
    handle = Toolkit.Window.Create(hints);
    context = Toolkit.OpenGL.CreateFromWindow(handle);

    Toolkit.OpenGL.SetCurrentContext(context);
    OpenTK.Graphics.GLLoader.LoadBindings(Toolkit.OpenGL.GetBindingsContext(context));

    DisplayHandle display = Toolkit.Display.OpenPrimary();
    MonitorInfo monitorInfo = Monitors.GetPrimaryMonitor();
    Console.WriteLine($"MonitorInfo: {monitorInfo.CurrentVideoMode.Width}, {monitorInfo.CurrentVideoMode.Height}");

    Toolkit.Window.SetMode(handle, WindowMode.Normal);
    Toolkit.Window.SetClientSize(handle, new Vector2i(800, 600));
    GL.Viewport(0, 0, 800, 600);

    EventQueue.EventRaised += HandleEvents;

    // Center the window on the screen
    Window.SetPosition(Monitor.GetCenter() - Window.GetSize() / 2);
  }

  public static void SetPosition(Vector2i position) {
    Toolkit.Window.SetClientPosition(handle, position);
  }

  public static Vector2i GetSize() {
    return size;
  }

  public static void SwapBuffers() {
    Toolkit.OpenGL.SwapBuffers(context);
  }

  public static void HandleEvents(PalHandle? handle, PlatformEventType type, EventArgs args) {
    switch (args) {
      case CloseEventArgs closeEvent:
        Toolkit.Window.Destroy(Window.handle);
        break;

      case KeyDownEventArgs keydownEvent:
        // Console.WriteLine($"Keydown event: {keydownEvent}");
        if (keydownEvent.Key == Key.Escape) {
          Toolkit.Window.Destroy(Window.handle);
        }
        break;

      default:
        // Console.WriteLine($"Unknown event type: {type}");
        break;
    }
  }
}
