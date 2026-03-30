using OpenTK.Mathematics;
using OpenTK.Platform;

public static class Monitor {
  public static Vector2i GetResolution() {
    DisplayHandle display = Toolkit.Display.OpenPrimary();
    Toolkit.Display.GetResolution(display, out int width, out int height);
    return new Vector2i(width, height);
  }

  public static Vector2i GetCenter() {
    Vector2i resolution = GetResolution();
    return new Vector2i(resolution.X / 2, resolution.Y / 2);
  }
}
