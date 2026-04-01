using OpenTK.Graphics.OpenGL;
using StbImageSharp;

internal class Texture {
  public int handle;
  public ImageResult image = null!;

  public Texture() { }

  public Texture(string filePath) {
    using FileStream stream = File.OpenRead(filePath);
    this.Buffer(stream);
  }

  public void Buffer(Stream stream) {
    if (this.image != null) {
      GL.DeleteTexture(this.handle);
    }

    this.handle = GL.GenTexture();
    GL.BindTexture(TextureTarget.Texture2d, this.handle);
    StbImage.stbi_set_flip_vertically_on_load(1);
    this.image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
    StbImage.stbi_set_flip_vertically_on_load(0);
    GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, this.image.Width,
    this.image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, this.image.Data);
    GL.GenerateMipmap(TextureTarget.Texture2d);
  }
}
