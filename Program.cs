using ImGuiNET;
using OpenTK.Core.Utility;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;
using OpenTK.Windowing.Desktop;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

public static class Program {
  private static bool drawLines = true;
  private static bool showImGuiDemoWindow;

  public static void Main(string[] args) {
    Window.Init(1800, 1200, "Uptime");
    Debug.Init();

    ImGuiApp.Init();

    Shader shader = new Shader("vert.glsl", "frag.glsl");

    GL.ClearColor(0.2f, 0.3f, 0.4f, 1.0f);
    GL.Enable(EnableCap.DepthTest);
    GL.Enable(EnableCap.CullFace);
    GL.CullFace(TriangleFace.Back);
    GL.FrontFace(FrontFaceDirection.Ccw);
    GL.Enable(EnableCap.PolygonOffsetLine);
    GL.PolygonOffset(-1, 0);

    Vector3[] verticies = [
      (-0.5f, -0.5f, -0.5f), (0.5f, -0.5f, -0.5f),(0.5f,  0.5f, -0.5f),
      (0.5f,  0.5f, -0.5f), (-0.5f,  0.5f, -0.5f),(-0.5f, -0.5f, -0.5f),

      (-0.5f, -0.5f,  0.5f), (0.5f, -0.5f,  0.5f),(0.5f,  0.5f,  0.5f),
      (0.5f,  0.5f,  0.5f), (-0.5f,  0.5f,  0.5f),(-0.5f, -0.5f,  0.5f),

      (-0.5f,  0.5f,  0.5f), (-0.5f,  0.5f, -0.5f),(-0.5f, -0.5f, -0.5f),
      (-0.5f, -0.5f, -0.5f), (-0.5f, -0.5f,  0.5f),(-0.5f,  0.5f,  0.5f),

      (0.5f,  0.5f,  0.5f), (0.5f,  0.5f, -0.5f),(0.5f, -0.5f, -0.5f),
      (0.5f, -0.5f, -0.5f), (0.5f, -0.5f,  0.5f),(0.5f,  0.5f,  0.5f),

      (-0.5f, -0.5f, -0.5f), (0.5f, -0.5f, -0.5f),(0.5f, -0.5f,  0.5f),
      (0.5f, -0.5f,  0.5f), (-0.5f, -0.5f,  0.5f),(-0.5f, -0.5f, -0.5f),

      (-0.5f,  0.5f, -0.5f), (0.5f,  0.5f, -0.5f),(0.5f,  0.5f,  0.5f),
      (0.5f,  0.5f,  0.5f), (-0.5f,  0.5f,  0.5f),(-0.5f,  0.5f, -0.5f),
    ];

    Vector2[] uvCoords = [
    (0.0f, 0.0f), (1.0f, 0.0f), (1.0f, 1.0f),
    (1.0f, 1.0f), (0.0f, 1.0f), (0.0f, 0.0f),
    (0.0f, 0.0f), (1.0f, 0.0f), (1.0f, 1.0f),
    (1.0f, 1.0f), (0.0f, 1.0f), (0.0f, 0.0f),
    (1.0f, 0.0f), (1.0f, 1.0f), (0.0f, 1.0f),
    (0.0f, 1.0f), (0.0f, 0.0f), (1.0f, 0.0f),
    (1.0f, 0.0f), (1.0f, 1.0f), (0.0f, 1.0f),
    (0.0f, 1.0f), (0.0f, 0.0f), (1.0f, 0.0f),
    (0.0f, 1.0f), (1.0f, 1.0f), (1.0f, 0.0f),
    (1.0f, 0.0f), (0.0f, 0.0f), (0.0f, 1.0f),
    (0.0f, 1.0f), (1.0f, 1.0f), (1.0f, 0.0f),
    (1.0f, 0.0f), (0.0f, 0.0f), (0.0f, 1.0f),
];

    float[] data = new float[verticies.Length * 5];
    for (int d = 0, i = 0; d < data.Length; d += 5, i++) {
      data[d + 0] = verticies[i].X;
      data[d + 1] = verticies[i].Y;
      data[d + 2] = verticies[i].Z;
      data[d + 3] = uvCoords[i].X;
      data[d + 4] = uvCoords[i].Y;
    }

    int vao = GL.GenVertexArray();
    GL.BindVertexArray(vao);

    int vbo = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
    GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsage.StaticDraw);

    uint position = (uint)GL.GetAttribLocation(shader.id, "position");
    GL.EnableVertexAttribArray(position);
    GL.VertexAttribPointer(position, 3, VertexAttribPointerType.Float, false, sizeof(float) * 5, 0);

    uint uv = (uint)GL.GetAttribLocation(shader.id, "uv");
    GL.EnableVertexAttribArray(uv);
    GL.VertexAttribPointer(uv, 2, VertexAttribPointerType.Float, false, sizeof(float) * 5, sizeof(float) * 3);

    string brickPath = Path.Combine(AppContext.BaseDirectory, "assets", "red_brick_diff_1k.png");
    Texture brick = new Texture();
    using (FileStream brickTextureStream = File.OpenRead(brickPath))
      brick.Buffer(brickTextureStream);

    int uRotation = GL.GetUniformLocation(shader.id, "rotation");
    int uView = GL.GetUniformLocation(shader.id, "view");
    int uProjection = GL.GetUniformLocation(shader.id, "projection");
    int udrawLineFlag = GL.GetUniformLocation(shader.id, "drawLineFlag");
    int uTex = GL.GetUniformLocation(shader.id, "tex");
    int uObjectId = GL.GetUniformLocation(shader.id, "objectId");
    int uHoveredObject = GL.GetUniformLocation(shader.id, "hoveredObject");
    int uHoverEnabled = GL.GetUniformLocation(shader.id, "hoverEnabled");

    Camera cam = Window.Camera;

    while (true) {
      Toolkit.Window.ProcessEvents(false);
      if (Toolkit.Window.IsWindowDestroyed(Window.handle)) break;

      if (Window.CameraResetRequested) {
        cam.ResetToDefaultView();
        Window.CameraResetRequested = false;
      }

      cam.Move(Window.Move);

      Toolkit.Window.GetClientSize(Window.handle, out Vector2i clientPx);
      int vw = clientPx.X > 0 ? clientPx.X : 800;
      int vh = clientPx.Y > 0 ? clientPx.Y : 600;
      GL.Viewport(0, 0, vw, vh);

      ImGuiApp.NewFrame(vw, vh);

      // Draw
      {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        Matrix4 orient = Matrix4.CreateRotationX(-2.2f) * Matrix4.CreateRotationY(0.7f);
        Matrix4 cube0Model = orient * Matrix4.CreateTranslation(-1.25f, 0f, 0f);
        Matrix4 cube1Model = orient * Matrix4.CreateTranslation(1.25f, 0f, 0f);
        Matrix4[] cubeModels = [cube0Model, cube1Model];

        Matrix4 proj = cam.Projection;
        Matrix4 view = cam.View;

        int hoveredObject = -1;
        if (!Window.Grabbed)
          hoveredObject = CubeHoverPick.PickObject(
            Window.ClientPointer, vw, vh, cam.Position, view, proj, cubeModels);

        GL.UniformMatrix4f(uView, 1, true, ref view);
        GL.UniformMatrix4f(uProjection, 1, true, ref proj);
        GL.Uniform1i(uHoveredObject, hoveredObject);
        GL.Uniform1i(uHoverEnabled, Window.Grabbed ? 0 : 1);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2d, brick.handle);
        GL.Uniform1i(uTex, 0);
        GL.Uniform1i(udrawLineFlag, 0);

        for (int i = 0; i < cubeModels.Length; i++) {
          Matrix4 model = cubeModels[i];
          GL.UniformMatrix4f(uRotation, 1, true, ref model);
          GL.Uniform1i(uObjectId, i);
          GL.DrawArrays(PrimitiveType.Triangles, 0, verticies.Length);
        }

        if (drawLines) {
          GL.Uniform1i(udrawLineFlag, 1);
          GL.Uniform1i(uHoverEnabled, 0);
          GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
          for (int i = 0; i < cubeModels.Length; i++) {
            Matrix4 model = cubeModels[i];
            GL.UniformMatrix4f(uRotation, 1, true, ref model);
            GL.Uniform1i(uObjectId, i);
            GL.DrawArrays(PrimitiveType.Triangles, 0, verticies.Length);
          }
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
