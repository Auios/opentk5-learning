using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

public struct Face {
  public Vector3[] verticies;
  public Vector2[] uvCoords;
}

public class Cube {
  public Face[] faces;

  public static Face FrontFace() {
    return new Face {
      verticies = [
        (-0.5f, -0.5f, -0.5f), (0.5f, -0.5f, -0.5f),(0.5f,  0.5f, -0.5f),
        (0.5f,  0.5f, -0.5f), (-0.5f,  0.5f, -0.5f),(-0.5f, -0.5f, -0.5f),
      ],
      uvCoords = [
        (0.0f, 0.0f), (1.0f, 0.0f), (1.0f, 1.0f),
        (1.0f, 1.0f), (0.0f, 1.0f), (0.0f, 0.0f),
      ],
    };
  }
  public static readonly Vector3[] verticies = [
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

  public static readonly Vector2[] uvCoords = [
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

  public int vao;

  public static float[] BuildVertexArray() {
    float[] data = new float[verticies.Length * 5];

    int i = 0;
    for (int d = 0; d < data.Length; d += 5) {
      data[d + 0] = verticies[i].X;
      data[d + 1] = verticies[i].Y;
      data[d + 2] = verticies[i].Z;

      data[d + 3] = uvCoords[i].X;
      data[d + 4] = uvCoords[i].Y;
      i++;
    }

    return data;
  }

  public void BindVao(int shaderId) {
    this.vao = GL.GenVertexArray();
    GL.BindVertexArray(this.vao);

    float[] data = BuildVertexArray();

    int vbo = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
    GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsage.StaticDraw);

    uint position = (uint)GL.GetAttribLocation(shaderId, "position");
    GL.EnableVertexAttribArray(position);
    GL.VertexAttribPointer(position, 3, VertexAttribPointerType.Float, false, sizeof(float) * 5, 0);

    uint uv = (uint)GL.GetAttribLocation(shaderId, "uv");
    GL.EnableVertexAttribArray(uv);
    GL.VertexAttribPointer(uv, 2, VertexAttribPointerType.Float, false, sizeof(float) * 5, sizeof(float) * 3);
  }
}

