using ImGuiNET;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;

public static class Program {
  private static bool drawLines = true;
  private static bool showImGuiDemoWindow;

  public static void Main(string[] args) {
    Window.Init(1800, 1200, "Uptime");
    Debug.Init();

    ImGuiApp.Init();

    Shader shader = new("vert.glsl", "frag.glsl");

    string monkeyGlbPath = Path.Combine(AppContext.BaseDirectory, "assets", "models", "monkey.glb");
    GltfModel monkey = new();
    monkey.Load(monkeyGlbPath);
    monkey.BindVao(shader.id);

    Cube cube = new();
    cube.BindVao(shader.id);

    GL.ClearColor(0.2f, 0.3f, 0.4f, 1.0f);
    GL.Enable(EnableCap.DepthTest);
    GL.Enable(EnableCap.CullFace);
    GL.CullFace(TriangleFace.Back);
    GL.FrontFace(FrontFaceDirection.Ccw);
    GL.Enable(EnableCap.PolygonOffsetLine);
    GL.PolygonOffset(-1, 0);

    string brickTexturePath = Path.Combine(AppContext.BaseDirectory, "assets", "textures/red_brick_diff_1k.png");
    Texture brickTexture = new(brickTexturePath);
    string spawnSoundPath = Path.Combine(AppContext.BaseDirectory, "assets", "sounds", "spawn.wav");
    using Sound spawnSound = new(spawnSoundPath);
    spawnSound.Play();

    int uTranslation = GL.GetUniformLocation(shader.id, "translation");
    int uView = GL.GetUniformLocation(shader.id, "view");
    int uProjection = GL.GetUniformLocation(shader.id, "projection");
    int udrawLineFlag = GL.GetUniformLocation(shader.id, "drawLineFlag");
    int uTex = GL.GetUniformLocation(shader.id, "tex");
    int uObjectId = GL.GetUniformLocation(shader.id, "objectId");
    int uHoveredObject = GL.GetUniformLocation(shader.id, "hoveredObject");
    int uHoverEnabled = GL.GetUniformLocation(shader.id, "hoverEnabled");

    Camera cam = Window.camera;

    while (true) {
      Toolkit.Window.ProcessEvents(false);
      if (Toolkit.Window.IsWindowDestroyed(Window.handle)) break;

      if (Window.cameraResetRequested) {
        cam.ResetToDefaultView();
        spawnSound.Play();
        Window.cameraResetRequested = false;
      }

      cam.Move(Window.move);

      Toolkit.Window.GetClientSize(Window.handle, out Vector2i clientPx);
      int vw = clientPx.X > 0 ? clientPx.X : 800;
      int vh = clientPx.Y > 0 ? clientPx.Y : 600;
      GL.Viewport(0, 0, vw, vh);

      ImGuiApp.NewFrame(vw, vh);

      // Draw
      {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        Matrix4 monkeyModel = Matrix4.CreateRotationX(-2.2f) * Matrix4.CreateRotationY(0.7f);
        Matrix4 cubeModel = Matrix4.CreateRotationX(2.2f) * Matrix4.CreateRotationY(0.7f)
          * Matrix4.CreateTranslation(2f, 0f, 0f);

        Matrix4 proj = cam.Projection;
        Matrix4 view = cam.View;

        int hoveredObject = -1;

        GL.UniformMatrix4f(uView, 1, true, ref view);
        GL.UniformMatrix4f(uProjection, 1, true, ref proj);
        GL.Uniform1i(uHoveredObject, hoveredObject);
        GL.Uniform1i(uHoverEnabled, Window.Grabbed ? 0 : 1);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2d, brickTexture.handle);
        GL.Uniform1i(uTex, 0);
        GL.Uniform1i(udrawLineFlag, 0);

        Matrix4 translation = monkeyModel;
        GL.UniformMatrix4f(uTranslation, 1, true, ref translation);
        GL.BindVertexArray(monkey.Vao);
        GL.Uniform1i(uObjectId, 0);
        GL.DrawElements(PrimitiveType.Triangles, monkey.VertexCount, DrawElementsType.UnsignedInt, 0);

        translation = cubeModel;
        GL.UniformMatrix4f(uTranslation, 1, true, ref translation);
        GL.BindVertexArray(cube.vao);
        GL.Uniform1i(uObjectId, 1);
        GL.DrawArrays(PrimitiveType.Triangles, 0, Cube.verticies.Length);

        if (drawLines) {
          GL.Uniform1i(udrawLineFlag, 1);
          GL.Uniform1i(uHoverEnabled, 0);
          GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

          translation = monkeyModel;
          GL.UniformMatrix4f(uTranslation, 1, true, ref translation);
          GL.BindVertexArray(monkey.Vao);
          GL.Uniform1i(uObjectId, 0);
          GL.DrawElements(PrimitiveType.Triangles, monkey.VertexCount, DrawElementsType.UnsignedInt, 0);

          translation = cubeModel;
          GL.UniformMatrix4f(uTranslation, 1, true, ref translation);
          GL.BindVertexArray(cube.vao);
          GL.Uniform1i(uObjectId, 1);
          GL.DrawArrays(PrimitiveType.Triangles, 0, Cube.verticies.Length);

          GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
          GL.Uniform1i(udrawLineFlag, 0);
        }

        float yawDeg = cam.Yaw * (180f / MathF.PI);
        float pitchDeg = cam.Pitch * (180f / MathF.PI);
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(8f, 8f), ImGuiCond.Always, new System.Numerics.Vector2(0f, 0f));
        ImGui.SetNextWindowBgAlpha(0.72f);
        ImGui.Begin("Camera",
          ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoMove
          | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings);
        ImGui.Text($"Position: ({cam.Position.X:F2}, {cam.Position.Y:F2}, {cam.Position.Z:F2})");
        ImGui.Text($"Rotation: yaw {yawDeg:F1} deg, pitch {pitchDeg:F1} deg");
        ImGui.End();

        if (ImGui.IsKeyPressed(ImGuiKey.H))
          drawLines = !drawLines;
        if (ImGui.IsKeyPressed(ImGuiKey.E))
          showImGuiDemoWindow = !showImGuiDemoWindow;
        if (showImGuiDemoWindow)
          ImGui.ShowDemoWindow(ref showImGuiDemoWindow);

        ImGuiApp.RenderDrawData();

        Window.SwapBuffers();
      }

      Thread.Sleep(1);
    }

    ImGuiApp.Shutdown();
  }
}
