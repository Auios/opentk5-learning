using OpenTK.Core.Utility;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;
using OpenTK.Windowing.Desktop;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

public static class Program {
  private static bool drawLines = true;

  public static void Main(string[] args) {
    Window.Init(800, 600, "Uptime");
    Debug.Init();

    Shader shader = new Shader("vert.glsl", "frag.glsl");

    GL.ClearColor(0.2f, 0.3f, 0.4f, 1.0f);
    GL.Enable(EnableCap.DepthTest);
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

    // GL.Enable(EnableCap.CullFace);

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

    Camera cam = Window.Camera;

    while (true) {
      Toolkit.Window.ProcessEvents(false);
      if (Toolkit.Window.IsWindowDestroyed(Window.handle)) break;

      if (Window.CameraResetRequested) {
        cam.Position = new Vector3(0f, 0.5f, 1.5f);
        cam.Pitch = -0.3f;
        cam.Yaw = 0f;
        Window.Move = Vector3.Zero;
        Window.ReleaseMouseLook();
        cam.Update();
        Window.CameraResetRequested = false;
      }

      cam.Move(Window.Move);

      // Draw
      {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        Matrix4 rotation = Matrix4.CreateRotationX(-2.2f) * Matrix4.CreateRotationY(0.7f);
        Matrix4 proj = cam.Projection;
        Matrix4 view = cam.View;
        GL.UniformMatrix4f(uRotation, 1, true, ref rotation);
        GL.UniformMatrix4f(uView, 1, true, ref view);
        GL.UniformMatrix4f(uProjection, 1, true, ref proj);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2d, brick.handle);
        GL.Uniform1i(uTex, 0);
        GL.Uniform1i(udrawLineFlag, 0);
        GL.DrawArrays(PrimitiveType.Triangles, 0, verticies.Length);
        if (drawLines) {
          GL.Uniform1i(udrawLineFlag, 1);
          GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
          GL.DrawArrays(PrimitiveType.Triangles, 0, verticies.Length);
          GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
          GL.Uniform1i(udrawLineFlag, 0);
        }
        Window.SwapBuffers();
      }

      Thread.Sleep(1);
    }
  }
}
