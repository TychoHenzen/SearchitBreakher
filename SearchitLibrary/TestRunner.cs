using System;
using System.IO;
using SearchitLibrary.Tests;

namespace SearchitLibrary;

/// <summary>
/// Simple test runner for validating voxel loading functionality
/// </summary>
public class TestRunner
{
    /// <summary>
    /// Runs a test to validate GOX file loading against a text file reference
    /// </summary>
    public static void RunGoxLoadingTest(string textFilePath, string goxFilePath)
    {
        Console.WriteLine($"Testing GOX file loading");
        Console.WriteLine($"Text file: {textFilePath}");
        Console.WriteLine($"GOX file: {goxFilePath}");
        
        if (!File.Exists(textFilePath))
        {
            Console.WriteLine($"Error: Text file not found at {textFilePath}");
            return;
        }
        
        if (!File.Exists(goxFilePath))
        {
            Console.WriteLine($"Error: GOX file not found at {goxFilePath}");
            return;
        }
        
        bool success = GoxFileTests.TestGoxLoading(textFilePath, goxFilePath);
        
        if (success)
        {
            Console.WriteLine("Test passed: GOX loading implementation is working correctly!");
        }
        else
        {
            Console.WriteLine("Test failed: GOX loading implementation has issues.");
            Console.WriteLine("Please check the log for details on the differences.");
        }
    }
    
    /// <summary>
    /// Runs the test with the standard test files from the SearchitTest project
    /// </summary>
    public static void RunStandardGoxLoadingTest()
    {
        // Try to locate the test files
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string textFilePath = FindFile(baseDir, "Test.txt");
        string goxFilePath = FindFile(baseDir, "Test.gox");
        
        if (textFilePath == null || goxFilePath == null)
        {
            Console.WriteLine("Could not locate test files. Please provide the paths explicitly.");
            return;
        }
        
        RunGoxLoadingTest(textFilePath, goxFilePath);
    }
    
    /// <summary>
    /// Utility method to find a file by searching up through parent directories
    /// </summary>
    private static string FindFile(string startDir, string fileName)
    {
        // Check current directory
        string filePath = Path.Combine(startDir, fileName);
        if (File.Exists(filePath))
            return filePath;
        
        // Check TestVoxels directory
        filePath = Path.Combine(startDir, "TestVoxels", fileName);
        if (File.Exists(filePath))
            return filePath;
        
        // Check SearchitTest/TestVoxels directory
        filePath = Path.Combine(startDir, "SearchitTest", "TestVoxels", fileName);
        if (File.Exists(filePath))
            return filePath;
        
        // Move up one directory and try again (limited to 5 levels)
        DirectoryInfo parent = Directory.GetParent(startDir);
        if (parent != null && parent.FullName != startDir)
        {
            string result = FindFile(parent.FullName, fileName);
            if (result != null)
                return result;
        }
        
        return null;
    }
}
