using System;
using System.IO;
using System.Text.Json;

namespace SearchitLibrary
{
    public class Constants
    {
        public const int ChunkSize = 32;
        public static int VoxelCount => ChunkSize * ChunkSize * ChunkSize;
        
        public float LookSpeed { get; set; } = 0.1f;
        public float MoveSpeed { get; set; } = 0.1f;
    }
}