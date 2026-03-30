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

    Vector3[] verticies = [(-0.5f, -0.5f, 0.0f), (0.5f, -0.5f, 0.0f), (-0.5f, 0.5f, 0.0f),
    (0.5f, -0.5f, 0.0f), (0.5f, 0.5f, 0.0f), (-0.5f, 0.5f, 0.0f)];

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

    while (true) {
      Toolkit.Window.ProcessEvents(false);
      if (Toolkit.Window.IsWindowDestroyed(Window.handle)) break;

      GL.Clear(ClearBufferMask.ColorBufferBit);
      GL.DrawArrays(PrimitiveType.Triangles, 0, verticies.Length);
      Window.SwapBuffers();

      Thread.Sleep(1);
    }
  }
}
