using OpenTK.Mathematics;

public class Camera {
  Vector3 position;
  float yaw;
  float pitch;

  Vector3 front;
  Vector3 right;
  Vector3 up;

  Matrix4 projection;
  Matrix4 view;

  float near = 0.1f;
  float far = 100f;

  float moveSpeed = 0.05f;
  float lookSpeed = 2.5f;

  /// <summary>Default eye position; cube sits at origin with no translation on the model matrix.</summary>
  public static Vector3 DefaultEye => new Vector3(0f, 0.5f, 1.5f);

  public Camera(float aspectRatio) {
    projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), aspectRatio, near, far);

    OrientToward(Vector3.Zero, DefaultEye);

    Update();
  }

  /// <summary>Sets yaw/pitch so the camera at <paramref name="eye"/> looks toward <paramref name="target"/>.</summary>
  public void OrientToward(Vector3 target, Vector3 eye) {
    position = eye;
    Vector3 forward = Vector3.Normalize(target - position);
    pitch = MathF.Asin(Math.Clamp(forward.Y, -1f, 1f));
    yaw = MathF.Atan2(forward.X, forward.Z);
  }

  /// <summary>Reset to default position and facing the cube at the origin.</summary>
  public void ResetToDefaultView() {
    OrientToward(Vector3.Zero, DefaultEye);
    Update();
  }

  public Vector3 Position {
    get => position;
    set => position = value;
  }

  public float Pitch {
    get => pitch;
    set => pitch = value;
  }

  public float Yaw {
    get => yaw;
    set => yaw = value;
  }

  public void Look(Vector2 delta) {
    yaw -= delta.X * lookSpeed;
    pitch -= delta.Y * lookSpeed;

    yaw = MathHelper.NormalizeRadians(yaw);
    pitch = Math.Clamp(pitch, -1.5f, 1.5f);

    Update();
  }

  public void Move(Vector3 move) {
    // Window uses W -> Move.Z -= 1 (negative Z = "forward" input). That must map to +front, so use -move.Z * front.
    // Strafe uses move.X * right so A (negative X) gives -right (strafe left).
    Vector3 direction = (move.X * right) + (move.Y * up) - (move.Z * front);

    position += direction * moveSpeed;

    Update();
  }

  public void Update() {
    float x = MathF.Cos(pitch) * MathF.Sin(yaw);
    float y = MathF.Sin(pitch);
    float z = MathF.Cos(pitch) * MathF.Cos(yaw);

    front = Vector3.Normalize(new Vector3(x, y, z));
    right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
    up = Vector3.Normalize(Vector3.Cross(right, front));

    view = Matrix4.LookAt(position, position + front, up);
  }

  public Matrix4 Projection => projection;
  public Matrix4 View => view;
}
