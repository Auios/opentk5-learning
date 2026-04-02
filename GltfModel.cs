using System.Buffers.Binary;
using System.IO;
using glTFLoader;
using glTFLoader.Schema;
using OpenTK.Graphics.OpenGL;

internal sealed class GltfModel {
  private float[] data;
  private uint[] indices;
  private int vao;
  private int indexCount;

  /// <summary>Loads the first mesh in the file. Supports .glb (embedded BIN) and .gltf with a sibling .bin URI.</summary>
  public void Load(string file) {
    string fullPath = Path.GetFullPath(file);
    Gltf model = Interface.LoadModel(fullPath);
    ReadOnlySpan<byte> buffer = GltfBufferBytes.Load(fullPath, model);

    Mesh mesh = model.Meshes[0];
    MeshPrimitive primitive = mesh.Primitives[0];

    Accessor positionAccessor = model.Accessors[primitive.Attributes["POSITION"]];
    Accessor textureAccessor = model.Accessors[primitive.Attributes["TEXCOORD_0"]];

    this.data = new float[positionAccessor.Count * 5];

    BufferView positionView = model.BufferViews[positionAccessor.BufferView!.Value];
    BufferView textureView = model.BufferViews[textureAccessor.BufferView!.Value];

    int positionStride = positionView.ByteStride ?? 0;
    if (positionStride == 0)
      positionStride = sizeof(float) * 3;

    int textureStride = textureView.ByteStride ?? 0;
    if (textureStride == 0)
      textureStride = sizeof(float) * 2;

    int p = positionView.ByteOffset + positionAccessor.ByteOffset;
    int t = textureView.ByteOffset + textureAccessor.ByteOffset;
    for (int i = 0; i < this.data.Length; i += 5) {
      this.data[i + 0] = ReadSingle(buffer, p + 0);
      this.data[i + 1] = ReadSingle(buffer, p + 4);
      this.data[i + 2] = ReadSingle(buffer, p + 8);
      p += positionStride;

      this.data[i + 3] = ReadSingle(buffer, t + 0);
      this.data[i + 4] = ReadSingle(buffer, t + 4);
      t += textureStride;
    }

    Accessor indexAccessor = model.Accessors[primitive.Indices!.Value];
    BufferView indexView = model.BufferViews[indexAccessor.BufferView!.Value];
    int indexBase = indexView.ByteOffset + indexAccessor.ByteOffset;
    int indexStride = indexView.ByteStride ?? 0;

    this.indexCount = indexAccessor.Count;
    this.indices = new uint[this.indexCount];

    if (indexStride == 0) {
      indexStride = indexAccessor.ComponentType switch {
        Accessor.ComponentTypeEnum.UNSIGNED_BYTE => 1,
        Accessor.ComponentTypeEnum.UNSIGNED_SHORT => 2,
        Accessor.ComponentTypeEnum.UNSIGNED_INT => 4,
        _ => 4,
      };
    }

    for (int i = 0; i < this.indexCount; i++) {
      int at = indexBase + (i * indexStride);
      this.indices[i] = indexAccessor.ComponentType switch {
        Accessor.ComponentTypeEnum.UNSIGNED_SHORT => BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(at, 2)),
        Accessor.ComponentTypeEnum.UNSIGNED_INT => BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(at, 4)),
        Accessor.ComponentTypeEnum.UNSIGNED_BYTE => buffer[at],
        _ => BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(at, 4)),
      };
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

    int ebo = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
    GL.BufferData(BufferTarget.ElementArrayBuffer, this.indices.Length * sizeof(uint), this.indices, BufferUsage.StaticDraw);
  }

  public int Vao => this.vao;

  /// <summary>Index count for <see cref="GL.DrawElements"/> (matches the tutorial name <c>VertexCount</c>).</summary>
  public int VertexCount => this.indexCount;

  private static float ReadSingle(ReadOnlySpan<byte> buffer, int index) {
    return BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(index, 4));
  }
}

/// <summary>Reads the glTF buffer bytes for Khronos <c>glTF2Loader</c> (NuGet build omits <c>Interface.LoadBinaryBuffer</c> helpers).</summary>
file static class GltfBufferBytes {
  private const uint gltfHeaderMagic = 0x46546C67;
  private const uint chunkBin = 0x004E4942;

  public static ReadOnlySpan<byte> Load(string filePath, Gltf model) {
    if (string.Equals(Path.GetExtension(filePath), ".glb", StringComparison.OrdinalIgnoreCase))
      return LoadGlbBinChunk(filePath);

    if (model.Buffers is { Length: > 0 } buffers && buffers[0].Uri is string uri && !string.IsNullOrEmpty(uri)) {
      string dir = Path.GetDirectoryName(filePath)!;
      string binPath = Path.Combine(dir, Uri.UnescapeDataString(uri));
      return File.ReadAllBytes(binPath);
    }

    throw new InvalidOperationException("Expected .glb or .gltf with Buffer[0].Uri pointing to a .bin file.");
  }

  private static byte[] LoadGlbBinChunk(string filePath) {
    using FileStream fs = File.OpenRead(filePath);
    using BinaryReader br = new(fs);
    uint magic = br.ReadUInt32();
    if (magic != gltfHeaderMagic)
      throw new InvalidDataException("Not a binary .glb (wrong magic).");

    _ = br.ReadUInt32();
    _ = br.ReadUInt32();

    while (fs.Position < fs.Length) {
      uint chunkLength = br.ReadUInt32();
      uint chunkType = br.ReadUInt32();
      byte[] data = br.ReadBytes((int)chunkLength);
      if (chunkType == chunkBin)
        return data;
    }

    throw new InvalidDataException(".glb file has no BIN chunk.");
  }
}
