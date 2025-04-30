using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SearchitLibrary.Graphics;
using System;
using System.Collections.Generic;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace SearchitBreakher.Graphics;

public class ChunkRenderer
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly BasicEffect _basicEffect;
    private VertexPositionColor[] _vertices;
    private int[] _indices;
    private MonoGameCamera _camera;
    private VoxelChunk _currentChunk;
    private readonly VoxelShader _shader;
    
    // Color mapping based on voxel type
    private readonly Dictionary<byte, System.Numerics.Vector3[]> _colorMap;
    
    // Number of vertices and indices per visible voxel face
    private const int VerticesPerFace = 4;
    private const int IndicesPerFace = 6; // 2 triangles, 3 indices each
    
    public ChunkRenderer(GraphicsDevice graphicsDevice, MonoGameCamera camera)
    {
        _graphicsDevice = graphicsDevice;
        _camera = camera;
        _shader = new VoxelShader();
        
        // Create the color map for different voxel types
        _colorMap = new Dictionary<byte, System.Numerics.Vector3[]>
        {
            // Default/empty voxel (shouldn't be rendered)
            { 0, new[] {
                new System.Numerics.Vector3(0.0f, 0.0f, 0.0f),
                new System.Numerics.Vector3(0.0f, 0.0f, 0.0f),
                new System.Numerics.Vector3(0.0f, 0.0f, 0.0f),
                new System.Numerics.Vector3(0.0f, 0.0f, 0.0f),
                new System.Numerics.Vector3(0.0f, 0.0f, 0.0f),
                new System.Numerics.Vector3(0.0f, 0.0f, 0.0f)
            }},
            
            // Type 1: Red-themed voxel
            { 1, new[] {
                new System.Numerics.Vector3(1.0f, 0.2f, 0.2f), // Front
                new System.Numerics.Vector3(0.9f, 0.1f, 0.1f), // Back
                new System.Numerics.Vector3(0.8f, 0.0f, 0.0f), // Left
                new System.Numerics.Vector3(0.7f, 0.0f, 0.0f), // Right
                new System.Numerics.Vector3(0.6f, 0.0f, 0.0f), // Top
                new System.Numerics.Vector3(0.5f, 0.0f, 0.0f)  // Bottom
            }},
            
            // Type 2: Green-themed voxel
            { 2, new[] {
                new System.Numerics.Vector3(0.2f, 1.0f, 0.2f), // Front
                new System.Numerics.Vector3(0.1f, 0.9f, 0.1f), // Back
                new System.Numerics.Vector3(0.0f, 0.8f, 0.0f), // Left
                new System.Numerics.Vector3(0.0f, 0.7f, 0.0f), // Right
                new System.Numerics.Vector3(0.0f, 0.6f, 0.0f), // Top
                new System.Numerics.Vector3(0.0f, 0.5f, 0.0f)  // Bottom
            }},
            
            // Type 3: Blue-themed voxel
            { 3, new[] {
                new System.Numerics.Vector3(0.2f, 0.2f, 1.0f), // Front
                new System.Numerics.Vector3(0.1f, 0.1f, 0.9f), // Back
                new System.Numerics.Vector3(0.0f, 0.0f, 0.8f), // Left
                new System.Numerics.Vector3(0.0f, 0.0f, 0.7f), // Right
                new System.Numerics.Vector3(0.0f, 0.0f, 0.6f), // Top
                new System.Numerics.Vector3(0.0f, 0.0f, 0.5f)  // Bottom
            }},
            
            // Type 4: Yellow-themed voxel
            { 4, new[] {
                new System.Numerics.Vector3(1.0f, 1.0f, 0.2f), // Front
                new System.Numerics.Vector3(0.9f, 0.9f, 0.1f), // Back
                new System.Numerics.Vector3(0.8f, 0.8f, 0.0f), // Left
                new System.Numerics.Vector3(0.7f, 0.7f, 0.0f), // Right
                new System.Numerics.Vector3(0.6f, 0.6f, 0.0f), // Top
                new System.Numerics.Vector3(0.5f, 0.5f, 0.0f)  // Bottom
            }},
            
            // Type 5: Cyan-themed voxel
            { 5, new[] {
                new System.Numerics.Vector3(0.2f, 1.0f, 1.0f), // Front
                new System.Numerics.Vector3(0.1f, 0.9f, 0.9f), // Back
                new System.Numerics.Vector3(0.0f, 0.8f, 0.8f), // Left
                new System.Numerics.Vector3(0.0f, 0.7f, 0.7f), // Right
                new System.Numerics.Vector3(0.0f, 0.6f, 0.6f), // Top
                new System.Numerics.Vector3(0.0f, 0.5f, 0.5f)  // Bottom
            }},
            
            // Type 6: Magenta-themed voxel
            { 6, new[] {
                new System.Numerics.Vector3(1.0f, 0.2f, 1.0f), // Front
                new System.Numerics.Vector3(0.9f, 0.1f, 0.9f), // Back
                new System.Numerics.Vector3(0.8f, 0.0f, 0.8f), // Left
                new System.Numerics.Vector3(0.7f, 0.0f, 0.7f), // Right
                new System.Numerics.Vector3(0.6f, 0.0f, 0.6f), // Top
                new System.Numerics.Vector3(0.5f, 0.0f, 0.5f)  // Bottom
            }}
        };

        // Create the effect
        _basicEffect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            View = camera.GetViewMatrix(),
            Projection = camera.GetProjectionMatrix(),
            World = Matrix.Identity
        };

        // Initialize with an empty chunk
        _vertices = Array.Empty<VertexPositionColor>();
        _indices = Array.Empty<int>();
    }

    public void SetChunk(VoxelChunk chunk)
    {
        _currentChunk = chunk;
        GenerateMesh();
    }

    // Regenerate the entire mesh for the chunk
    private void GenerateMesh()
    {
        if (_currentChunk == null)
        {
            _vertices = Array.Empty<VertexPositionColor>();
            _indices = Array.Empty<int>();
            return;
        }
        
        // Calculate how many vertices and indices we'll need
        int maxFaces = CalculateVisibleFaces(_currentChunk);
        _vertices = new VertexPositionColor[maxFaces * VerticesPerFace];
        _indices = new int[maxFaces * IndicesPerFace];
        
        int vertexOffset = 0;
        int indexOffset = 0;
        
        // Iterate through all voxels in the chunk
        for (int x = 0; x < VoxelChunk.ChunkSize; x++)
        {
            for (int y = 0; y < VoxelChunk.ChunkSize; y++)
            {
                for (int z = 0; z < VoxelChunk.ChunkSize; z++)
                {
                    byte voxelType = _currentChunk.GetVoxel(x, y, z);
                    
                    // Skip empty voxels
                    if (voxelType == 0)
                    {
                        continue;
                    }
                    
                    // Get the face colors for this voxel type
                    System.Numerics.Vector3[] faceColors = _colorMap.ContainsKey(voxelType) 
                        ? _colorMap[voxelType] 
                        : _colorMap[1]; // Default to type 1 if not found
                    
                    // Calculate the voxel position in world space
                    System.Numerics.Vector3 voxelPosition = new System.Numerics.Vector3(
                        _currentChunk.Position.X + x, 
                        _currentChunk.Position.Y + y, 
                        _currentChunk.Position.Z + z
                    );
                    
                    // Add visible faces
                    AddVisibleFaces(voxelPosition, voxelType, faceColors, x, y, z, ref vertexOffset, ref indexOffset);
                }
            }
        }
        
        // Resize arrays to actual used size
        if (vertexOffset < _vertices.Length)
        {
            Array.Resize(ref _vertices, vertexOffset);
            Array.Resize(ref _indices, indexOffset);
        }
    }
    
    // Calculate how many visible faces we need to render
    private int CalculateVisibleFaces(VoxelChunk chunk)
    {
        int visibleFaces = 0;
        
        for (int x = 0; x < VoxelChunk.ChunkSize; x++)
        {
            for (int y = 0; y < VoxelChunk.ChunkSize; y++)
            {
                for (int z = 0; z < VoxelChunk.ChunkSize; z++)
                {
                    byte voxelType = chunk.GetVoxel(x, y, z);
                    
                    // Skip empty voxels
                    if (voxelType == 0)
                    {
                        continue;
                    }
                    
                    // Check each adjacent voxel
                    // Front face (negative Z)
                    if (z == 0 || chunk.GetVoxel(x, y, z - 1) == 0)
                    {
                        visibleFaces++;
                    }
                    
                    // Back face (positive Z)
                    if (z == VoxelChunk.ChunkSize - 1 || chunk.GetVoxel(x, y, z + 1) == 0)
                    {
                        visibleFaces++;
                    }
                    
                    // Left face (negative X)
                    if (x == 0 || chunk.GetVoxel(x - 1, y, z) == 0)
                    {
                        visibleFaces++;
                    }
                    
                    // Right face (positive X)
                    if (x == VoxelChunk.ChunkSize - 1 || chunk.GetVoxel(x + 1, y, z) == 0)
                    {
                        visibleFaces++;
                    }
                    
                    // Bottom face (negative Y)
                    if (y == 0 || chunk.GetVoxel(x, y - 1, z) == 0)
                    {
                        visibleFaces++;
                    }
                    
                    // Top face (positive Y)
                    if (y == VoxelChunk.ChunkSize - 1 || chunk.GetVoxel(x, y + 1, z) == 0)
                    {
                        visibleFaces++;
                    }
                }
            }
        }
        
        return visibleFaces;
    }
    
    // Add visible faces for a single voxel
    private void AddVisibleFaces(
        System.Numerics.Vector3 position, 
        byte voxelType, 
        System.Numerics.Vector3[] faceColors, 
        int x, int y, int z, 
        ref int vertexOffset, 
        ref int indexOffset)
    {
        float voxelSize = 1.0f; // Unit size for voxels
        float halfSize = voxelSize / 2.0f;
        System.Numerics.Vector3 cameraPosition = new(
            _camera.Position.X, 
            _camera.Position.Y, 
            _camera.Position.Z
        );
        
        // Front face (negative Z)
        if (z == 0 || _currentChunk.GetVoxel(x, y, z - 1) == 0)
        {
            AddFace(
                [
                    position + new System.Numerics.Vector3(-halfSize, -halfSize, -halfSize), // front bottom left (0)
                    position + new System.Numerics.Vector3(halfSize, -halfSize, -halfSize),  // front bottom right (1)
                    position + new System.Numerics.Vector3(-halfSize, halfSize, -halfSize),  // front top left (2)
                    position + new System.Numerics.Vector3(halfSize, halfSize, -halfSize)    // front top right (3)
                ],
                faceColors[0], // Front face color
                cameraPosition,
                vertexOffset,
                indexOffset
            );
            vertexOffset += VerticesPerFace;
            indexOffset += IndicesPerFace;
        }
        
        // Back face (positive Z)
        if (z == VoxelChunk.ChunkSize - 1 || _currentChunk.GetVoxel(x, y, z + 1) == 0)
        {
            AddFace(
                [
                    position + new System.Numerics.Vector3(halfSize, -halfSize, halfSize),   // back bottom right (4)
                    position + new System.Numerics.Vector3(-halfSize, -halfSize, halfSize),  // back bottom left (5)
                    position + new System.Numerics.Vector3(halfSize, halfSize, halfSize),    // back top right (6)
                    position + new System.Numerics.Vector3(-halfSize, halfSize, halfSize)    // back top left (7)
                ],
                faceColors[1], // Back face color
                cameraPosition,
                vertexOffset,
                indexOffset
            );
            vertexOffset += VerticesPerFace;
            indexOffset += IndicesPerFace;
        }
        
        // Left face (negative X)
        if (x == 0 || _currentChunk.GetVoxel(x - 1, y, z) == 0)
        {
            AddFace(
                [
                    position + new System.Numerics.Vector3(-halfSize, -halfSize, halfSize),  // back bottom left (8)
                    position + new System.Numerics.Vector3(-halfSize, -halfSize, -halfSize), // front bottom left (9)
                    position + new System.Numerics.Vector3(-halfSize, halfSize, halfSize),   // back top left (10)
                    position + new System.Numerics.Vector3(-halfSize, halfSize, -halfSize)   // front top left (11)
                ],
                faceColors[2], // Left face color
                cameraPosition,
                vertexOffset,
                indexOffset
            );
            vertexOffset += VerticesPerFace;
            indexOffset += IndicesPerFace;
        }
        
        // Right face (positive X)
        if (x == VoxelChunk.ChunkSize - 1 || _currentChunk.GetVoxel(x + 1, y, z) == 0)
        {
            AddFace(
                new[] {
                    position + new System.Numerics.Vector3(halfSize, -halfSize, -halfSize),  // front bottom right (12)
                    position + new System.Numerics.Vector3(halfSize, -halfSize, halfSize),   // back bottom right (13)
                    position + new System.Numerics.Vector3(halfSize, halfSize, -halfSize),   // front top right (14)
                    position + new System.Numerics.Vector3(halfSize, halfSize, halfSize)     // back top right (15)
                },
                faceColors[3], // Right face color
                cameraPosition,
                vertexOffset,
                indexOffset
            );
            vertexOffset += VerticesPerFace;
            indexOffset += IndicesPerFace;
        }
        
        // Top face (positive Y)
        if (y == VoxelChunk.ChunkSize - 1 || _currentChunk.GetVoxel(x, y + 1, z) == 0)
        {
            AddFace(
                new[] {
                    position + new System.Numerics.Vector3(-halfSize, halfSize, -halfSize),  // front top left (16)
                    position + new System.Numerics.Vector3(halfSize, halfSize, -halfSize),   // front top right (17)
                    position + new System.Numerics.Vector3(-halfSize, halfSize, halfSize),   // back top left (18)
                    position + new System.Numerics.Vector3(halfSize, halfSize, halfSize)     // back top right (19)
                },
                faceColors[4], // Top face color
                cameraPosition,
                vertexOffset,
                indexOffset
            );
            vertexOffset += VerticesPerFace;
            indexOffset += IndicesPerFace;
        }
        
        // Bottom face (negative Y)
        if (y == 0 || _currentChunk.GetVoxel(x, y - 1, z) == 0)
        {
            AddFace(
                new[] {
                    position + new System.Numerics.Vector3(-halfSize, -halfSize, -halfSize), // front bottom left (20)
                    position + new System.Numerics.Vector3(halfSize, -halfSize, -halfSize),  // front bottom right (21)
                    position + new System.Numerics.Vector3(-halfSize, -halfSize, halfSize),  // back bottom left (22)
                    position + new System.Numerics.Vector3(halfSize, -halfSize, halfSize)    // back bottom right (23)
                },
                faceColors[5], // Bottom face color
                cameraPosition,
                vertexOffset,
                indexOffset
            );
            vertexOffset += VerticesPerFace;
            indexOffset += IndicesPerFace;
        }
    }
    
    // Add a face to the mesh with the appropriate indices
    private void AddFace(
        System.Numerics.Vector3[] vertices, 
        System.Numerics.Vector3 color, 
        System.Numerics.Vector3 cameraPosition, 
        int vertexOffset, 
        int indexOffset)
    {
        // Add vertices with appropriate shading
        for (int i = 0; i < vertices.Length; i++)
        {
            // Apply distance-based shading to the color
            System.Numerics.Vector3 shadedColor = _shader.ApplyDistanceShading(
                color,
                vertices[i],
                cameraPosition
            );
            
            // Create the vertex
            _vertices[vertexOffset + i] = new VertexPositionColor(
                new Vector3(vertices[i].X, vertices[i].Y, vertices[i].Z),
                new Color(shadedColor.X, shadedColor.Y, shadedColor.Z)
            );
        }
        
        // Add indices for the two triangles of the face (CCW winding)
        // Triangle 1
        _indices[indexOffset] = vertexOffset;
        _indices[indexOffset + 1] = vertexOffset + 1;
        _indices[indexOffset + 2] = vertexOffset + 2;
        
        // Triangle 2
        _indices[indexOffset + 3] = vertexOffset + 2;
        _indices[indexOffset + 4] = vertexOffset + 1;
        _indices[indexOffset + 5] = vertexOffset + 3;
    }

    public void UpdateCamera(MonoGameCamera camera)
    {
        _camera = camera;
        _basicEffect.View = camera.GetViewMatrix();
        _basicEffect.Projection = camera.GetProjectionMatrix();
        
        // Update the mesh with new camera position (for shading)
        if (_currentChunk != null)
        {
            GenerateMesh();
        }
    }

    public void Draw()
    {
        // Check if we have anything to draw
        if (_vertices.Length == 0 || _indices.Length == 0)
        {
            return;
        }
        
        // Set render state
        _graphicsDevice.RasterizerState = new RasterizerState
        {
            CullMode = CullMode.CullCounterClockwiseFace,  // Enable standard backface culling
            FillMode = FillMode.Solid
        };
        
        // Ensure depth buffer is enabled for proper z-ordering
        _graphicsDevice.DepthStencilState = new DepthStencilState
        {
            DepthBufferEnable = true,
            DepthBufferFunction = CompareFunction.LessEqual
        };

        // Apply the effect
        foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            // Draw the chunk using an index buffer
            _graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                _vertices,
                0,                  // vertex buffer offset
                _vertices.Length,   // vertex count
                _indices,           // index buffer
                0,                  // index buffer offset
                _indices.Length / 3 // primitive count
            );
        }
    }
}
