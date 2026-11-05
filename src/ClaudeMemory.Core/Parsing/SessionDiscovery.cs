using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClaudeMemory.Core.Parsing;

/// <summary>
/// Finds transcript files, ported from claude_memory.py:1020-1028 (<c>discover</c>).
/// </summary>
public static class SessionDiscovery
{
    /// <summary>
    /// Lists non-empty <c>*.jsonl</c> files under <paramref name="projectsDir"/> (or
    /// <c>&lt;root&gt;/projects</c>), oldest first, ties broken by file name.
    /// </summary>
    public static List<string> Discover(string root, string? projectsDir)
    {
        var baseDir = string.IsNullOrEmpty(projectsDir) ? System.IO.Path.Combine(root, "projects") : projectsDir;

        var files = new List<(string Path, DateTime WriteUtc)>();
        if (Directory.Exists(baseDir))
        {
            foreach (var path in Directory.EnumerateFiles(baseDir, "*.jsonl", SearchOption.AllDirectories))
            {
                try
                {
                    var info = new FileInfo(path);
                    if (info.Length > 0) files.Add((path, info.LastWriteTimeUtc));
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }

        // Read each file's write time once; OrderBy would otherwise stat the file once per
        // comparison, which matters when a project has thousands of transcripts.
        return files
            .OrderBy(f => f.WriteUtc)
            .ThenBy(f => System.IO.Path.GetFileName(f.Path), StringComparer.Ordinal)
            .Select(f => f.Path)
            .ToList();
    }
}
