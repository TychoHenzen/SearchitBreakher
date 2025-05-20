using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SearchitLibrary.Graphics;

public static class ChunkLoader
{
    // Offsets for the 8 sections in a GOX file
    private static readonly Vector3[] Offsets =
    {
        new(0, 0, 1), // section 1
        new(1, 0, 1), // section 2 
        new(1, 0, 0), // section 4
        new(0, 0, 0), // section 3
        new(0, 1, 0), // section 5
        new(0, 1, 1), // section 6
        new(1, 1, 1), // section 7
        new(1, 1, 0) // section 8
    };


    // Loads a chunk from a .gox file
    public static VoxelChunk? LoadGoxFile(string filePath, Vector3 position)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Could not find .gox file", filePath);
        }

        try
        {
            // Initialize voxel data array
            var voxelData = new byte[Constants.ChunkSize * Constants.ChunkSize * Constants.ChunkSize];

            using var stream = File.OpenRead(filePath);
            using BinaryReader reader = new(stream);

            // Actually parse the GOX file format
            var index = 0;
            while (stream.Position < stream.Length && index < 8)
            {
                if (SeekToMarker(reader, "BL16"))
                {
                    // Read the PNG data size
                    var fileSize = reader.ReadUInt32();

                    // Read the PNG data
                    var pngData = reader.ReadBytes((int)fileSize);

                    // Load the PNG data using ImageSharp
                    using (var img = Image.Load<Rgba32>(pngData))
                    {
                        // Process the image data for this section
                        LoadImage(index, img, voxelData);
                    }

                    index++;
                }
                else
                {
                    // If we can't find the marker, break out of the loop
                    break;
                }
            }

            // Create and return the chunk
            return new VoxelChunk(position, voxelData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading .gox file: {ex.Message}");
            return null;
        }
    }

    private static bool SeekToMarker(BinaryReader reader, string marker)
    {
        var startPos = reader.BaseStream.Position;

        // We'll search for up to 1MB to find the marker
        var maxSearchBytes = Math.Min(1024 * 1024, reader.BaseStream.Length - startPos);

        for (int i = 0; i < maxSearchBytes; i++)
        {
            var pos = reader.BaseStream.Position;

            // Make sure we have enough bytes to read the marker
            if (pos + marker.Length > reader.BaseStream.Length)
            {
                return false;
            }

            // Try to match the marker
            var match = true;
            foreach (var t in marker)
            {
                var b = reader.ReadByte();
                if (b == t) continue;
                match = false;
                break;
            }

            if (match)
            {
                return true;
            }

            // If it didn't match, go back to after the first byte we checked and continue
            reader.BaseStream.Position = pos + 1;
        }

        return false;
    }

    private static void LoadImage(int sectionIndex, Image<Rgba32> img, byte[] voxelData)
    {
        // Iterate through all pixels in the image (assumed to be 64x64)
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                ParsePixel(sectionIndex, img, voxelData, x, y);
            }
        }
    }

    private static void ParsePixel(int sectionIndex, Image<Rgba32> img, byte[] voxelData, int coordX, int coordY)
    {
        // Calculate the corresponding position in 3D space based on the pixel coordinates
        int posIndex = coordX + 64 * coordY;
        int px = posIndex % 16;
        posIndex /= 16;
        int pz = posIndex % 16;
        posIndex /= 16;
        int py = posIndex;

        // Apply the section offset
        var offset = Offsets[sectionIndex];
        int vx = (int)(px + offset.X * 16);
        int vy = (int)(py + offset.Y * 16);
        int vz = (int)(pz + offset.Z * 16);

        // Get the pixel color
        Rgba32 pixel = img[coordX, coordY];

        // Convert to ARGB format
        int color = (pixel.A << 24) | (pixel.R << 16) | (pixel.G << 8) | pixel.B;

        // Skip transparent pixels
        if ((color & 0xFF000000) == 0)
        {
            return;
        }

        // Get only RGB components
        int rgbColor = color & 0x00FFFFFF;

        // Map color to voxel type
        byte voxelType = rgbColor.MapColorToVoxelType();

        // If the voxel type is 0 (empty/white), skip
        if (voxelType == 0)
        {
            return;
        }

        // Calculate the index in the voxel array
        int voxelIndex = Helpers.GetIndex(vx, vy, vz);

        // Set the voxel data if the index is valid
        if (voxelIndex >= 0 && voxelIndex < voxelData.Length)
        {
            voxelData[voxelIndex] = voxelType;
        }
    }
}