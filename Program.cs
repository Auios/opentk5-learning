using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Core.Utility;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;
using OpenTK.Windowing.Desktop;
using Vector3 = OpenTK.Mathematics.Vector3;

public static class Program {
  public static void Main(string[] args) {
    Window.Init(800, 600, "Uptime");
    Debug.Init();

    Shader shader = new Shader("vert.glsl", "frag.glsl");

    GL.ClearColor(0.2f, 0.3f, 0.4f, 1.0f);
    GL.Enable(EnableCap.DepthTest);

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

    // GL.Enable(EnableCap.CullFace);
    // verticies = verticies.Reverse().ToArray();

    int vao = GL.GenVertexArray();
    GL.BindVertexArray(vao);

    int vbo = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
    GL.BufferData(BufferTarget.ArrayBuffer, verticies.Length * sizeof(float) * 3, verticies, BufferUsage.StaticDraw);

    uint position = (uint)GL.GetAttribLocation(shader.id, "position");
    GL.EnableVertexAttribArray(position);
    GL.VertexAttribPointer(position, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
    int uTransformation = GL.GetUniformLocation(shader.id, "transform");
    int uProjection = GL.GetUniformLocation(shader.id, "projection");

    // Matrix4 projection = Matrix4.CreateOrthographic(2.66f, 2f, 0.01f, 100f);
    Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
      MathHelper.DegreesToRadians(90f), 1.33f, 0.1f, 100f);

    float angle = 0f;

    while (true) {
      Toolkit.Window.ProcessEvents(false);
      if (Toolkit.Window.IsWindowDestroyed(Window.handle)) break;

      // Draw
      {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        Matrix4 transform = Matrix4.CreateRotationX(-angle)
          * Matrix4.CreateRotationY(angle / 2f)
          * Matrix4.CreateTranslation(0f, 0f, -2f)
          * Matrix4.CreateScale(1.5f, 1.5f, 2f);
        GL.UniformMatrix4f(uProjection, 1, true, in projection);
        GL.UniformMatrix4f(uTransformation, 1, true, ref transform);
        angle += 0.01f;
        angle = MathHelper.ClampRadians(angle);
        GL.DrawArrays(PrimitiveType.Triangles, 0, verticies.Length);
        Window.SwapBuffers();
      }

      Thread.Sleep(1);
    }
  }
}
