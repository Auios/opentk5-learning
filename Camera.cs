using OpenTK.Mathematics;

public class Camera {
  Vector3 position;
  float yaw;
  float pitch;

  Matrix4 projection;
  Matrix4 transform;
  Matrix4 view;

  float near = 0.1f;
  float far = 100f;

  float moveSpeed = 0.05f;
  float lookSpeed = 2.5f;

  public Camera(float aspectRatio) {
    position = new Vector3(0f, 0.5f, 1.5f);
    yaw = 0f;
    pitch = -0.3f;

    projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), aspectRatio, near, far);

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
    Vector3 right = transform.Row0.Xyz;
    Vector3 up = transform.Row1.Xyz;
    Vector3 front = transform.Row2.Xyz;

    Vector3 direction = (move.X * right) + (move.Y * up) + (move.Z * front);

    position += direction * moveSpeed;

    Update();
  }

  public void Update() {
    Matrix3 rotation =
      Matrix3.CreateRotationX(pitch) *
      Matrix3.CreateRotationY(yaw);

    transform = new Matrix4(rotation);
    transform.Row3 = new Vector4(position, 1f);

    view = transform.Inverted();
  }

  public Matrix4 Projection => projection;
  public Matrix4 View => view;
}
