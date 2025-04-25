using SearchitLibrary.Graphics;
using System;
using System.Numerics;

namespace SearchitTest;

public class CameraTests
{
    [Test]
    public void Camera_ShouldCreateWithCorrectParameters()
    {
        // Arrange
        var position = new Vector3(0, 0, 5);
        var target = new Vector3(0, 0, 0);
        var up = new Vector3(0, 1, 0);
        var fieldOfView = 0.78f; // ~45 degrees
        var aspectRatio = 1.5f;
        var nearPlane = 0.1f;
        var farPlane = 100f;

        // Act
        var camera = new Camera(position, target, up, fieldOfView, aspectRatio, nearPlane, farPlane);

        // Assert
        Assert.That(camera.Position, Is.EqualTo(position));
        Assert.That(camera.Target, Is.EqualTo(target));
        Assert.That(camera.Up, Is.EqualTo(up));
        Assert.That(camera.FieldOfView, Is.EqualTo(fieldOfView));
        Assert.That(camera.AspectRatio, Is.EqualTo(aspectRatio));
        Assert.That(camera.NearPlane, Is.EqualTo(nearPlane));
        Assert.That(camera.FarPlane, Is.EqualTo(farPlane));
    }

    [Test]
    public void GetViewMatrix_ShouldReturnLookAtMatrix()
    {
        // Arrange
        var position = new Vector3(0, 0, 5);
        var target = new Vector3(0, 0, 0);
        var up = new Vector3(0, 1, 0);
        var camera = new Camera(position, target, up, 0.78f, 1.5f, 0.1f, 100f);

        // Act
        var viewMatrix = camera.GetViewMatrix();

        // Assert
        var expectedMatrix = Matrix4x4.CreateLookAt(position, target, up);
        Assert.That(viewMatrix, Is.EqualTo(expectedMatrix));
    }

    [Test]
    public void GetProjectionMatrix_ShouldReturnPerspectiveMatrix()
    {
        // Arrange
        var fieldOfView = 0.78f; // ~45 degrees
        var aspectRatio = 1.5f;
        var nearPlane = 0.1f;
        var farPlane = 100f;
        var camera = new Camera(new Vector3(0, 0, 5), new Vector3(0, 0, 0), new Vector3(0, 1, 0),
            fieldOfView, aspectRatio, nearPlane, farPlane);

        // Act
        var projectionMatrix = camera.GetProjectionMatrix();

        // Assert
        var expectedMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
            fieldOfView, aspectRatio, nearPlane, farPlane);
        Assert.That(projectionMatrix, Is.EqualTo(expectedMatrix));
    }

    [Test]
    public void ChangingYaw_ShouldUpdateTargetPosition()
    {
        // Arrange
        var position = new Vector3(0, 0, 5);
        var target = new Vector3(0, 0, 0);
        var up = new Vector3(0, 1, 0);
        var camera = new Camera(position, target, up, 0.78f, 1.5f, 0.1f, 100f);
        var initialYaw = camera.Yaw;

        // Act
        camera.Yaw = initialYaw + MathF.PI / 2.0f; // Rotate 90 degrees

        // Assert
        Assert.That(camera.Yaw, Is.EqualTo(initialYaw + MathF.PI / 2.0f));
        Assert.That(camera.Target, Is.Not.EqualTo(target)); // Target should have changed

        // Verify that the direction changes as expected
        var direction = Vector3.Normalize(camera.Target - camera.Position);

        // The direction should now be perpendicular to the initial direction
        // Since we started looking down the Z axis, a 90-degree yaw should have us looking
        // along either the positive or negative X axis
        Assert.That(Math.Abs(direction.X), Is.GreaterThan(0.9f).Within(0.1f)); // Mostly looking along X axis
        Assert.That(direction.Z, Is.InRange(-0.1f, 0.1f)); // Little to no Z component
    }

    [Test]
    public void ChangingPitch_ShouldUpdateTargetPosition()
    {
        // Arrange
        var position = new Vector3(0, 0, 5);
        var target = new Vector3(0, 0, 0);
        var up = new Vector3(0, 1, 0);
        var camera = new Camera(position, target, up, 0.78f, 1.5f, 0.1f, 100f);
        var initialPitch = camera.Pitch;

        // Act
        camera.Pitch = initialPitch + MathF.PI / 4.0f; // Rotate 45 degrees up

        // Assert
        Assert.That(camera.Pitch, Is.EqualTo(initialPitch + MathF.PI / 4.0f));
        Assert.That(camera.Target, Is.Not.EqualTo(target)); // Target should have changed

        // Verify that the direction points upward
        var direction = Vector3.Normalize(camera.Target - camera.Position);
        Assert.That(direction.Y, Is.GreaterThan(0.5f)); // Looking upward significantly
    }

    [Test]
    public void Pitch_ShouldBeClampedToAvoidGimbalLock()
    {
        // Arrange
        var camera = new Camera(
            new Vector3(0, 0, 5),
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0),
            0.78f, 1.5f, 0.1f, 100f);

        // Act & Assert
        camera.Pitch = MathF.PI; // Try to look straight up and beyond
        Assert.That(camera.Pitch, Is.LessThan(MathF.PI / 2.0f)); // Should be clamped to less than 90 degrees

        camera.Pitch = -MathF.PI; // Try to look straight down and beyond
        Assert.That(camera.Pitch, Is.GreaterThan(-MathF.PI / 2.0f)); // Should be clamped to greater than -90 degrees
    }

    [Test]
    public void MoveForward_ShouldMovePositionAndTarget()
    {
        // Arrange
        var position = new Vector3(0, 0, 5);
        var target = new Vector3(0, 0, 0);
        var up = new Vector3(0, 1, 0);
        var camera = new Camera(position, target, up, 0.78f, 1.5f, 0.1f, 100f);
        var initialPosition = camera.Position;
        var initialTarget = camera.Target;
        var initialDirection = Vector3.Normalize(initialTarget - initialPosition);

        // Act
        camera.MoveForward(1.0f); // Move forward one unit

        // Assert
        // We should have moved along the -Z axis (toward the target)
        Assert.That(camera.Position.Z, Is.LessThan(initialPosition.Z));

        // Direction should remain the same
        var newDirection = Vector3.Normalize(camera.Target - camera.Position);
        Assert.That(Vector3.Dot(initialDirection, newDirection), Is.GreaterThan(0.99f));

        // The position should have moved in the direction we were facing
        var positionChange = camera.Position - initialPosition;
        Assert.That(positionChange.Z, Is.LessThan(0)); // Moved forward, so Z decreases
    }

    [Test]
    public void MoveRight_ShouldMovePositionAndTarget()
    {
        // Arrange
        var position = new Vector3(0, 0, 5);
        var target = new Vector3(0, 0, 0);
        var up = new Vector3(0, 1, 0);
        var camera = new Camera(position, target, up, 0.78f, 1.5f, 0.1f, 100f);
        var initialPosition = camera.Position;
        var initialTarget = camera.Target;
        var initialDirection = Vector3.Normalize(initialTarget - initialPosition);

        // Act
        camera.MoveRight(1.0f); // Move right one unit

        // Assert
        // We should have moved along the X axis (to the right)
        Assert.That(camera.Position.X, Is.GreaterThan(initialPosition.X));

        // Direction should remain the same
        var newDirection = Vector3.Normalize(camera.Target - camera.Position);
        Assert.That(Vector3.Dot(initialDirection, newDirection), Is.GreaterThan(0.99f));

        // The position should have moved perpendicular to the direction we were facing
        var positionChange = camera.Position - initialPosition;
        Assert.That(positionChange.X, Is.GreaterThan(0)); // Moved right, so X increases
        Assert.That(Math.Abs(positionChange.Z), Is.LessThan(0.1f)); // Minimal Z change
    }

    [Test]
    public void MoveUp_ShouldMovePositionAndTarget()
    {
        // Arrange
        var position = new Vector3(0, 0, 5);
        var target = new Vector3(0, 0, 0);
        var up = new Vector3(0, 1, 0);
        var camera = new Camera(position, target, up, 0.78f, 1.5f, 0.1f, 100f);
        var initialPosition = camera.Position;
        var initialTarget = camera.Target;
        var initialDirection = Vector3.Normalize(initialTarget - initialPosition);

        // Act
        camera.MoveUp(1.0f); // Move up one unit

        // Assert
        // We should have moved along the Y axis (upward)
        Assert.That(camera.Position.Y, Is.GreaterThan(initialPosition.Y));

        // Direction should remain the same
        var newDirection = Vector3.Normalize(camera.Target - camera.Position);
        Assert.That(Vector3.Dot(initialDirection, newDirection), Is.GreaterThan(0.99f));

        // The position should have moved upward
        var positionChange = camera.Position - initialPosition;
        Assert.That(positionChange.Y, Is.GreaterThan(0)); // Moved up, so Y increases
    }

    [Test]
    public void RelativeMovement_ShouldWorkAfterRotation()
    {
        // Arrange
        var camera = new Camera(
            new Vector3(0, 0, 5),
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0),
            0.78f, 1.5f, 0.1f, 100f);

        // Act
        // First rotate 90 degrees (now looking along X axis)
        var initialYaw = camera.Yaw;
        camera.Yaw = initialYaw + MathF.PI / 2.0f;

        var positionBeforeMove = camera.Position;

        // Then move forward
        camera.MoveForward(1.0f);

        // Assert
        // After rotating 90 degrees, forward is now along X axis
        // So Z should not change significantly, but X should decrease
        Assert.That(camera.Position.X, Is.Not.EqualTo(positionBeforeMove.X));
        Assert.That(Math.Abs(camera.Position.Z - positionBeforeMove.Z), Is.LessThan(0.1f));
    }
}