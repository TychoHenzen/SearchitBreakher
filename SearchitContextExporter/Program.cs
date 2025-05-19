using System.Collections.Specialized;
using System.Diagnostics;
using TextCopy;
using TiktokenSharp;
using Clipboard = System.Windows.Forms.Clipboard;

internal class Program
{
    public const bool tests = true;

    [STAThread]
    private static void Main(string[] args)
    {
        // Determine repository root by ascending four levels from the executable directory
        var exeDir = AppContext.BaseDirectory;
        var root = Path.GetFullPath(Path.Combine(exeDir, "..", "..", "..", ".."));
        var filePath = Path.Combine(root, "Searchit_context.txt");

        // Get the list of tracked files via git
        var trackedFiles = GetTrackedFiles(root);

        // Filter to only include common text-based files
        var extensions = new HashSet<string> { ".cs", ".csproj", ".txt", ".gox", ".json", ".xml", ".md" };
        var files = trackedFiles
            .Where(f => extensions.Contains(Path.GetExtension(f)))
            // Exclude IDE/editor settings
            .Where(f => !f.Contains(".vscode", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains(".config/", StringComparison.OrdinalIgnoreCase))
            // Exclude CLAUDE.md and empty/boilerplate docs
            .Where(f => !f.Contains("CLAUDE.md", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("README.md", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
            // (Optionally) skip tests if you don’t need them
            .Where(f => !f.StartsWith("SearchitContextExporter/", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (!tests)
            files = files.Where(f => !f.StartsWith("SearchitTest/", StringComparison.OrdinalIgnoreCase)).ToList();

        // Generate tree from filtered text files
        var treeLines = BuildTreeLines(files, root);
        File.WriteAllText(filePath, string.Join(Environment.NewLine, treeLines));
        File.AppendAllText(filePath, "\n");

        // Append contents of each filtered text file
        AppendFileContents(root, filePath, files);

        // Estimate and print LLM token count for the context file
        EstimateTokens(filePath);

        // Copy the file itself to the clipboard
        CopyFileToClipboard(filePath);
        Console.WriteLine($"Context file written to {filePath}.");
    }

    private static List<string> GetTrackedFiles(string root)
    {
        var psi = new ProcessStartInfo("git", "ls-files")
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var proc = Process.Start(psi);
        var output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();
        return output
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .ToList();
    }

    private static List<string> BuildTreeLines(List<string> relativePaths, string root)
    {
        var rootNode = new TreeNode(root);
        foreach (var rel in relativePaths)
        {
            var parts = rel.Split(new[] { '/', Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var node = rootNode;
            foreach (var name in parts)
            {
                var child = node.Children.FirstOrDefault(c => c.Name == name);
                if (child == null)
                {
                    child = new TreeNode(name);
                    node.Children.Add(child);
                }

                node = child;
            }
        }

        var lines = new List<string>();
        rootNode.WriteLines(string.Empty, lines);
        return lines;
    }

    private static void AppendFileContents(string root, string filePath, List<string> relativeFiles)
    {
        var BinaryExtensions = new HashSet<string> { ".gox" };
        using var writer = File.AppendText(filePath);
        foreach (var rel in relativeFiles)
            try
            {
                var relPath = rel.Replace('/', Path.DirectorySeparatorChar);
                var full = Path.Combine(root, relPath);
                writer.WriteLine($"=== Begin {rel} ===");
                if (!BinaryExtensions.Contains(Path.GetExtension(full)))
                    writer.Write(File.ReadAllText(full));
                else
                    writer.WriteLine("[Contents of binary file omitted]");

                writer.WriteLine($"=== End {rel} ===\n");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
    }

    private static void CopyFileToClipboard(string filePath)
    {
        try
        {
            var paths = new StringCollection { filePath };
            Clipboard.SetFileDropList(paths);
            Console.WriteLine("Copied file to clipboard (Windows file drop list).");
        }
        catch (Exception e)
        {
            ClipboardService.SetText(filePath);
            Console.WriteLine("Copied file path to clipboard as text. because " + e.Message);
        }
    }

    private static void EstimateTokens(string filePath)
    {
        try
        {
            var content = File.ReadAllText(filePath);
            // Use TiktokenSharp to get exact token count
            var encoder = TikToken.EncodingForModel("gpt-4o");
            var tokens = encoder.Encode(content);
            Console.WriteLine($"Estimated LLM tokens in context file: {tokens.Count}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to estimate tokens: {e.Message}");
        }
    }

    private class TreeNode
    {
        public TreeNode(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public List<TreeNode> Children { get; } = new();

        public void WriteLines(string indent, List<string> lines)
        {
            lines.Add($"{indent}{Name}{(Children.Any() ? "/" : string.Empty)}");
            foreach (var child in Children.OrderBy(c => c.Name))
                child.WriteLines(indent + "  ", lines);
        }
    }
}