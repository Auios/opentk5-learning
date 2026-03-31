using OpenTK.Core.Utility;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;
using OpenTK.Windowing.Desktop;

public static class Window {
  public static WindowHandle handle = null!;
  public static OpenGLContextHandle context = null!;
  public static Vector2i size;

  public static bool CameraResetRequested { get; set; }

  public static Camera Camera { get; private set; } = null!;

  private static Vector2 lastMouse = Vector2.Zero;
  private static bool mouseLookInitialized;

  private static CursorHandle defaultCursor = null!;
  private static bool grabbed;

  public static Vector3 Move = Vector3.Zero;

  public static void ResetMouseLook() {
    lastMouse = Vector2.Zero;
    mouseLookInitialized = false;
  }

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
    Toolkit.Window.SetBorderStyle(handle, WindowBorderStyle.FixedBorder);
    Toolkit.Window.SetClientSize(handle, new Vector2i(800, 600));
    GL.Viewport(0, 0, 800, 600);

    Toolkit.Window.GetClientSize(handle, out Vector2i clientSize);
    float aspect = clientSize.Y > 0 ? (float)clientSize.X / clientSize.Y : 1.33f;
    Camera = new Camera(aspect);

    defaultCursor = Toolkit.Cursor.Create(SystemCursorType.Default);

    EventQueue.EventRaised += HandleEvents;

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

  public static void ReleaseMouseLook() {
    grabbed = false;
    Toolkit.Window.SetCursorCaptureMode(handle, CursorCaptureMode.Normal);
    Toolkit.Window.SetCursor(handle, defaultCursor);
    ResetMouseLook();
  }

  public static void HandleEvents(PalHandle? palHandle, PlatformEventType type, EventArgs args) {
    switch (args) {
      case CloseEventArgs:
        Toolkit.Window.Destroy(Window.handle);
        break;

      case KeyDownEventArgs keyDown:
        if (keyDown.IsRepeat)
          break;
        switch (keyDown.Scancode) {
          case Scancode.LeftAlt:
            Toolkit.Window.SetCursorCaptureMode(handle, CursorCaptureMode.Locked);
            Toolkit.Window.SetCursor(handle, null);
            grabbed = true;
            break;
          case Scancode.W:
            Move.Z -= 1f;
            break;
          case Scancode.S:
            Move.Z += 1f;
            break;
          case Scancode.A:
            Move.X -= 1f;
            break;
          case Scancode.D:
            Move.X += 1f;
            break;
        }
        if (keyDown.Key == Key.Escape)
          Toolkit.Window.Destroy(Window.handle);
        break;

      case KeyUpEventArgs keyUp:
        switch (keyUp.Scancode) {
          case Scancode.LeftAlt:
            Toolkit.Window.SetCursorCaptureMode(handle, CursorCaptureMode.Normal);
            Toolkit.Window.SetCursor(handle, defaultCursor);
            grabbed = false;
            break;
          case Scancode.W:
            Move.Z += 1f;
            break;
          case Scancode.S:
            Move.Z -= 1f;
            break;
          case Scancode.A:
            Move.X += 1f;
            break;
          case Scancode.D:
            Move.X -= 1f;
            break;
        }
        break;

      case MouseButtonDownEventArgs mouseDown:
        if (mouseDown.Button == MouseButton.Button2)
          CameraResetRequested = true;
        break;

      case MouseMoveEventArgs mouseMove:
        if (!mouseLookInitialized) {
          lastMouse = mouseMove.ClientPosition;
          mouseLookInitialized = true;
          break;
        }
        Vector2 diff = mouseMove.ClientPosition - lastMouse;
        if (grabbed)
          Camera.Look(diff / 1000f);
        lastMouse = mouseMove.ClientPosition;
        break;

      default:
        break;
    }
  }
}
