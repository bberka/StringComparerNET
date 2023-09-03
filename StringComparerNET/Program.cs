// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;

namespace StringComparerNET;

public static class Program
{
  private static async Task Main(string[] args) {

    Console.WriteLine("Text File Comparer");
    Console.WriteLine("------------------");
    Console.WriteLine("Enter the paths to two text files to compare.");
    Console.WriteLine("The files will be compared line by line.");
    Console.WriteLine("File 1:");
    var file1Path = Console.ReadLine();
    
    Console.WriteLine("File 2:");
    var file2Path = Console.ReadLine();

    if (!File.Exists(file1Path) || !File.Exists(file2Path)) {
      Console.WriteLine("One or both files do not exist.");
      return;
    }

    var differences = await CompareFilesAsync(file1Path, file2Path);

    if (differences.Count == 0) {
      Console.WriteLine("Files are identical.");
    }
    else {
      Console.WriteLine("Differences between the files:");
      foreach (var diff in differences) {
        Console.WriteLine($"Line {diff.LineNumber}:");
        Console.WriteLine($"File 1: {diff.Line1}");
        Console.WriteLine($"File 2: {diff.Line2}");
        Console.WriteLine();
      }
    }
  }

  static async Task<List<LineDifference>> CompareFilesAsync(string file1Path, string file2Path) {
    var differences = new ConcurrentBag<LineDifference>();
    var file1Lines = await File.ReadAllLinesAsync(file1Path);
    var file2Lines = await File.ReadAllLinesAsync(file2Path);

      // .Select(x => x.Replace(" ", ""))
      // .Select(x => x.Replace(" ", ""))
    file1Lines = file1Lines.Where(x => !string.IsNullOrEmpty(x)).Reverse().ToArray();
    file2Lines = file2Lines.Where(x => !string.IsNullOrEmpty(x)).Reverse().ToArray();
    
    
    // Normalize the lines by removing white space and line endings
    var normalizedFile1Lines = NormalizeLines(file1Lines);
    var normalizedFile2Lines = NormalizeLines(file2Lines);

    await Task.WhenAll(
                       CompareLinesAsync(normalizedFile1Lines, normalizedFile2Lines, differences),
                       CompareLinesAsync(normalizedFile2Lines, normalizedFile1Lines, differences)
                      );

    return differences.ToList();
  }

  static async Task CompareLinesAsync(IEnumerable<string> lines1, IEnumerable<string> lines2, ConcurrentBag<LineDifference> differences) {
    int lineNumber = 0;
    using var enumerator1 = lines1.GetEnumerator();
    using var enumerator2 = lines2.GetEnumerator();

    while (enumerator1.MoveNext() && enumerator2.MoveNext()) {
      lineNumber++;
      if (enumerator1.Current != enumerator2.Current) {
        differences.Add(new LineDifference(lineNumber, enumerator1.Current, enumerator2.Current));
      }
    }

    // Handle any extra lines in the second file
    while (enumerator2.MoveNext()) {
      lineNumber++;
      differences.Add(new LineDifference(lineNumber, string.Empty, enumerator2.Current));
    }
  }

  static IEnumerable<string> NormalizeLines(IEnumerable<string> lines) {
    foreach (var line in lines) {
      // Remove white space and line endings (normalize the line)
      yield return new string(line.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }
  }
}

class LineDifference
{
  public int LineNumber { get; }
  public string Line1 { get; }
  public string Line2 { get; }

  public LineDifference(int lineNumber, string line1, string line2) {
    LineNumber = lineNumber;
    Line1 = line1;
    Line2 = line2;
  }
}