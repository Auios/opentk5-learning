using OpenTK.Mathematics;

public class Camera {
  private Vector3 position;
  private float yaw;
  private float pitch;

  private Vector3 front;
  private Vector3 right;
  private Vector3 up;

  private Matrix4 projection;
  private Matrix4 view;

  private float near = 0.1f;
  private float far = 100f;

  private float moveSpeed = 0.05f;
  private float lookSpeed = 2.5f;

  /// <summary>Default eye position; cube sits at origin with no translation on the model matrix.</summary>
  public static Vector3 DefaultEye => new Vector3(0f, 0.5f, 1.5f);

  public Camera(float aspectRatio) {
    this.projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), aspectRatio, this.near, this.far);

    this.OrientToward(Vector3.Zero, DefaultEye);

    this.Update();
  }

  /// <summary>Sets yaw/pitch so the camera at <paramref name="eye"/> looks toward <paramref name="target"/>.</summary>
  public void OrientToward(Vector3 target, Vector3 eye) {
    this.position = eye;
    Vector3 forward = Vector3.Normalize(target - this.position);
    this.pitch = MathF.Asin(Math.Clamp(forward.Y, -1f, 1f));
    this.yaw = MathF.Atan2(forward.X, forward.Z);
  }

  /// <summary>Reset to default position and facing the cube at the origin.</summary>
  public void ResetToDefaultView() {
    this.OrientToward(Vector3.Zero, DefaultEye);
    this.Update();
  }

  public Vector3 Position {
    get => this.position;
    set => this.position = value;
  }

  public float Pitch {
    get => this.pitch;
    set => this.pitch = value;
  }

  public float Yaw {
    get => this.yaw;
    set => this.yaw = value;
  }

  public void Look(Vector2 delta) {
    this.yaw -= delta.X * this.lookSpeed;
    this.pitch -= delta.Y * this.lookSpeed;

    this.yaw = MathHelper.NormalizeRadians(this.yaw);
    this.pitch = Math.Clamp(this.pitch, -1.5f, 1.5f);

    this.Update();
  }

  public void Move(Vector3 move) {
    // Window uses W -> Move.Z -= 1 (negative Z = "forward" input). That must map to +front, so use -move.Z * front.
    // Strafe uses move.X * right so A (negative X) gives -right (strafe left).
    Vector3 direction = (move.X * this.right) + (move.Y * this.up) - (move.Z * this.front);

    this.position += direction * this.moveSpeed;

    this.Update();
  }

  public void Update() {
    float x = MathF.Cos(this.pitch) * MathF.Sin(this.yaw);
    float y = MathF.Sin(this.pitch);
    float z = MathF.Cos(this.pitch) * MathF.Cos(this.yaw);

    this.front = Vector3.Normalize(new Vector3(x, y, z));
    this.right = Vector3.Normalize(Vector3.Cross(this.front, Vector3.UnitY));
    this.up = Vector3.Normalize(Vector3.Cross(this.right, this.front));

    this.view = Matrix4.LookAt(this.position, this.position + this.front, this.up);
  }

  public Matrix4 Projection => this.projection;
  public Matrix4 View => this.view;
}
