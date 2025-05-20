// using SearchitLibrary.Graphics;
// using System.Numerics;
//
// namespace SearchitTest;
//
// public class TriangleTests
// {
//     [Test]
//     public void CreateRGBTriangle_ShouldCreateTriangleWithCorrectVertices()
//     {
//         // Arrange & Act
//         var triangle = Triangle.CreateRGBTriangle();
//
//         // Assert
//         Assert.That(triangle, Is.Not.Null);
//         Assert.That(triangle.Vertices.Length, Is.EqualTo(3));
//         Assert.That(triangle.Colors.Length, Is.EqualTo(3));
//
//         // Verify vertices
//         Assert.That(triangle.Vertices[0], Is.EqualTo(new Vector3(-1.0f, -1.0f, 0.0f)));
//         Assert.That(triangle.Vertices[1], Is.EqualTo(new Vector3(1.0f, -1.0f, 0.0f)));
//         Assert.That(triangle.Vertices[2], Is.EqualTo(new Vector3(0.0f, 1.0f, 0.0f)));
//
//         // Verify colors
//         Assert.That(triangle.Colors[0], Is.EqualTo(new Vector3(1.0f, 0.0f, 0.0f))); // Red
//         Assert.That(triangle.Colors[1], Is.EqualTo(new Vector3(0.0f, 1.0f, 0.0f))); // Green
//         Assert.That(triangle.Colors[2], Is.EqualTo(new Vector3(0.0f, 0.0f, 1.0f))); // Blue
//     }
// }

