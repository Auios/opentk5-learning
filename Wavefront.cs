using JeremyAnsel.Media.WavefrontObj;
using OpenTK.Graphics.OpenGL;

internal sealed class Wavefront {
  private float[] data;
  private int vao;
  private int vertexCount;

  public void Load(string file) {
    ObjFile obj = ObjFile.FromFile(file);

    int totalVerts = 0;
    for (int v = 0; v < obj.Faces.Count; v++)
      totalVerts += obj.Faces[v].Vertices.Count;

    this.vertexCount = totalVerts;
    this.data = new float[this.vertexCount * 5];
    int i = 0;
    for (int v = 0; v < obj.Faces.Count; v++) {
      for (int f = 0; f < obj.Faces[v].Vertices.Count; f++) {
        ObjTriplet triplet = obj.Faces[v].Vertices[f];
        this.data[i++] = obj.Vertices[triplet.Vertex - 1].Position.X;
        this.data[i++] = obj.Vertices[triplet.Vertex - 1].Position.Y;
        this.data[i++] = obj.Vertices[triplet.Vertex - 1].Position.Z;

        if (triplet.Texture != 0 && obj.TextureVertices.Count > 0) {
          ObjVector3 tv = obj.TextureVertices[triplet.Texture - 1];
          this.data[i++] = tv.X;
          this.data[i++] = tv.Y;
        }
        else {
          this.data[i++] = 0f;
          this.data[i++] = 0f;
        }
      }
    }
  }

  public void BindVao(int shaderId) {
    this.vao = GL.GenVertexArray();
    GL.BindVertexArray(this.vao);

    int vbo = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
    GL.BufferData(BufferTarget.ArrayBuffer, this.data.Length * sizeof(float), this.data, BufferUsage.StaticDraw);

    uint position = (uint)GL.GetAttribLocation(shaderId, "position");
    GL.EnableVertexAttribArray(position);
    GL.VertexAttribPointer(position, 3, VertexAttribPointerType.Float, false, sizeof(float) * 5, 0);

    uint uv = (uint)GL.GetAttribLocation(shaderId, "uv");
    GL.EnableVertexAttribArray(uv);
    GL.VertexAttribPointer(uv, 2, VertexAttribPointerType.Float, false, sizeof(float) * 5, sizeof(float) * 3);
  }

  public int Vao => this.vao;

  public int VertexCount => this.vertexCount;
}
