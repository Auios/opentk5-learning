using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Opentk5Learning.ImGuiBackends;

public static class ImGuiApp {
  private static readonly Stopwatch frameClock = Stopwatch.StartNew();
  private static double lastTimeSeconds;

  public static void Init() {
    ImGui.CreateContext();
    ImGui.GetIO().Fonts.AddFontDefault();
    ImguiImplOpenGL3.Init();
    lastTimeSeconds = frameClock.Elapsed.TotalSeconds;
  }

  public static void Shutdown() {
    ImguiImplOpenGL3.Shutdown();
    ImGui.DestroyContext();
  }

  /// <summary>Call after window events are processed and before building any ImGui UI.</summary>
  public static void NewFrame(int viewportWidth, int viewportHeight) {
    ImGuiIOPtr io = ImGui.GetIO();
    double now = frameClock.Elapsed.TotalSeconds;
    float dt = (float)(now - lastTimeSeconds);
    lastTimeSeconds = now;
    if (dt <= 0f)
      dt = 1f / 60f;

    io.DisplaySize = new Vector2(viewportWidth, viewportHeight);
    io.DisplayFramebufferScale = Vector2.One;
    io.DeltaTime = dt;

    if (Window.Grabbed)
      io.ConfigFlags |= ImGuiConfigFlags.NoMouse;
    else
      io.ConfigFlags &= ~ImGuiConfigFlags.NoMouse;

    foreach ((ImGuiKey key, bool down) in Window.imGuiKeyEvents)
      io.AddKeyEvent(key, down);
    Window.imGuiKeyEvents.Clear();

    io.AddMousePosEvent(Window.clientPointer.X, Window.clientPointer.Y);
    io.AddMouseButtonEvent(0, Window.mouseLeft);
    io.AddMouseButtonEvent(1, Window.mouseRight);
    io.AddMouseButtonEvent(2, Window.mouseMiddle);
    io.AddMouseWheelEvent(Window.scrollAccum.X, Window.scrollAccum.Y);
    Window.scrollAccum = OpenTK.Mathematics.Vector2.Zero;

    ImguiImplOpenGL3.NewFrame();
    ImGui.NewFrame();
  }

  public static void RenderDrawData() {
    ImGui.Render();
    ImguiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());
  }
}
