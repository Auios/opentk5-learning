using OpenTK.Graphics.OpenGL;
using StbImageSharp;

class Texture {
  public int handle;
  public (int width, int height) Buffer(Stream stream) {
    handle = GL.GenTexture();
    GL.BindTexture(TextureTarget.Texture2d, handle);
    StbImage.stbi_set_flip_vertically_on_load(1);
    ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
    StbImage.stbi_set_flip_vertically_on_load(0);
    GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, image.Width,
    image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
    GL.GenerateMipmap(TextureTarget.Texture2d);
    return (image.Width, image.Height);
  }
}
