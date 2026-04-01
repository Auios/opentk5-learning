using OpenTK.Graphics.OpenGL;

public class Shader {
  public int id;
  public string vertexShaderSource;
  public string fragmentShaderSource;

  public Shader(string vertexShaderFilename, string fragmentShaderFilename) {
    string shaderDir = Path.Combine(AppContext.BaseDirectory, "shaders");

    this.vertexShaderSource = File.ReadAllText(Path.Combine(shaderDir, vertexShaderFilename));
    this.fragmentShaderSource = File.ReadAllText(Path.Combine(shaderDir, fragmentShaderFilename));

    int vertexHandle = GL.CreateShader(ShaderType.VertexShader);
    GL.ShaderSource(vertexHandle, this.vertexShaderSource);
    CompileShader(vertexHandle);

    int fragmentHandle = GL.CreateShader(ShaderType.FragmentShader);
    GL.ShaderSource(fragmentHandle, this.fragmentShaderSource);
    CompileShader(fragmentHandle);

    int shaderHandle = GL.CreateProgram();
    GL.AttachShader(shaderHandle, vertexHandle);
    GL.AttachShader(shaderHandle, fragmentHandle);
    LinkProgram(shaderHandle);

    GL.DetachShader(shaderHandle, vertexHandle);
    GL.DetachShader(shaderHandle, fragmentHandle);
    GL.DeleteShader(vertexHandle);
    GL.DeleteShader(fragmentHandle);

    this.id = shaderHandle;
    UseProgram(shaderHandle);
  }

  public static void CompileShader(int shaderHandle) {
    GL.CompileShader(shaderHandle);

    GL.GetShaderi(shaderHandle, ShaderParameterName.CompileStatus, out int code);
    if (code != (int)All.True) {
      GL.GetShaderInfoLog(shaderHandle, out var infoLog);
      throw new Exception($"Error compiling shader.{Environment.NewLine}{infoLog}");
    }
  }

  public static void LinkProgram(int shaderHandle) {
    GL.LinkProgram(shaderHandle);

    GL.GetProgrami(shaderHandle, ProgramProperty.LinkStatus, out int code);
    if (code != (int)All.True) {
      GL.GetProgramInfoLog(shaderHandle, out var infoLog);
      throw new Exception($"Error linking program.{Environment.NewLine}{infoLog}");
    }
  }

  public static void UseProgram(int shaderHandle) {
    GL.UseProgram(shaderHandle);
  }
}
