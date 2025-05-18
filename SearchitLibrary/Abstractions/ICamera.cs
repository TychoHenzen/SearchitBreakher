using System.Numerics;

namespace SearchitLibrary.Abstractions;

public interface ICamera
{
    Vector3 Position { get; }
    void Update(float deltaTime);
    Matrix4x4 GetViewMatrix();
    Matrix4x4 GetProjectionMatrix();
    void ApplyMouseDelta(Vector2 delta);
}