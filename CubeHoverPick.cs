using OpenTK.Mathematics;

/// <summary>Ray vs cube meshes for object hover.</summary>
public static class CubeHoverPick {
  private const float epsilon = 1e-5f;

  private static readonly Vector3[] cubeVertices = [
    (-0.5f, -0.5f, -0.5f), (0.5f, -0.5f, -0.5f), (0.5f, 0.5f, -0.5f),
    (0.5f, 0.5f, -0.5f), (-0.5f, 0.5f, -0.5f), (-0.5f, -0.5f, -0.5f),
    (-0.5f, -0.5f, 0.5f), (0.5f, -0.5f, 0.5f), (0.5f, 0.5f, 0.5f),
    (0.5f, 0.5f, 0.5f), (-0.5f, 0.5f, 0.5f), (-0.5f, -0.5f, 0.5f),
    (-0.5f, 0.5f, 0.5f), (-0.5f, 0.5f, -0.5f), (-0.5f, -0.5f, -0.5f),
    (-0.5f, -0.5f, -0.5f), (-0.5f, -0.5f, 0.5f), (-0.5f, 0.5f, 0.5f),
    (0.5f, 0.5f, 0.5f), (0.5f, 0.5f, -0.5f), (0.5f, -0.5f, -0.5f),
    (0.5f, -0.5f, -0.5f), (0.5f, -0.5f, 0.5f), (0.5f, 0.5f, 0.5f),
    (-0.5f, -0.5f, -0.5f), (0.5f, -0.5f, -0.5f), (0.5f, -0.5f, 0.5f),
    (0.5f, -0.5f, 0.5f), (-0.5f, -0.5f, 0.5f), (-0.5f, -0.5f, -0.5f),
    (-0.5f, 0.5f, -0.5f), (0.5f, 0.5f, -0.5f), (0.5f, 0.5f, 0.5f),
    (0.5f, 0.5f, 0.5f), (-0.5f, 0.5f, 0.5f), (-0.5f, 0.5f, -0.5f),
  ];

  /// <returns>Index of the closest hit object in <paramref name="objectModels"/>, or -1.</returns>
  public static int PickObject(
    Vector2 mouseClientTopLeft,
    int viewportW,
    int viewportH,
    Vector3 rayOrigin,
    Matrix4 view,
    Matrix4 projection,
    ReadOnlySpan<Matrix4> objectModels) {
    Vector3 rayDir = ScreenToWorldRay(mouseClientTopLeft, viewportW, viewportH, view, projection);
    float bestT = float.MaxValue;
    int bestObj = -1;

    for (int o = 0; o < objectModels.Length; o++) {
      Matrix4 model = objectModels[o];
      for (int tri = 0; tri < 12; tri++) {
        int b = tri * 3;
        Vector3 v0 = Vector3.TransformPosition(cubeVertices[b], model);
        Vector3 v1 = Vector3.TransformPosition(cubeVertices[b + 1], model);
        Vector3 v2 = Vector3.TransformPosition(cubeVertices[b + 2], model);

        if (RayTriangle(rayOrigin, rayDir, v0, v1, v2, out float t) && t >= epsilon && t < bestT) {
          bestT = t;
          bestObj = o;
        }
      }
    }

    return bestObj;
  }

  private static Vector3 ScreenToWorldRay(Vector2 mouseTopLeft, int w, int h, Matrix4 view, Matrix4 projection) {
    float mx = mouseTopLeft.X;
    float my = h - mouseTopLeft.Y;
    Matrix4 invVp = (view * projection).Inverted();
    Vector3 near = Vector3.Unproject(new Vector3(mx, my, 0f), 0f, 0f, w, h, 0f, 1f, invVp);
    Vector3 far = Vector3.Unproject(new Vector3(mx, my, 1f), 0f, 0f, w, h, 0f, 1f, invVp);
    return Vector3.Normalize(far - near);
  }

  private static bool RayTriangle(Vector3 orig, Vector3 dir, Vector3 v0, Vector3 v1, Vector3 v2, out float t) {
    Vector3 e1 = v1 - v0;
    Vector3 e2 = v2 - v0;
    Vector3 h = Vector3.Cross(dir, e2);
    float a = Vector3.Dot(e1, h);
    if (a > -epsilon && a < epsilon) {
      t = 0f;
      return false;
    }

    float f = 1f / a;
    Vector3 s = orig - v0;
    float u = f * Vector3.Dot(s, h);
    if (u < 0f || u > 1f) {
      t = 0f;
      return false;
    }

    Vector3 q = Vector3.Cross(s, e1);
    float v = f * Vector3.Dot(dir, q);
    if (v < 0f || u + v > 1f) {
      t = 0f;
      return false;
    }

    t = f * Vector3.Dot(e2, q);
    return t > epsilon;
  }
}
