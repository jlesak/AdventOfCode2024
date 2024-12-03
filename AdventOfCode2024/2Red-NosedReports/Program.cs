var fileName = "input.txt";

var reports = File.ReadAllLines(fileName)
    .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Select(int.Parse)
        .ToArray())
    .ToList();

var safeReportsPartOne = CountSafeReports(reports);
Console.WriteLine($"Part 1 - Number of safe reports: {safeReportsPartOne}");

var safeReportsPartTwo = CountSafeReportsWithDampener(reports);
Console.WriteLine($"Part 2 - Number of safe reports with Problem Dampener: {safeReportsPartTwo}");

// AnalyzeReportsInDetail(reports);
return;

static int CountSafeReports(List<int[]> reports) =>
    reports.Count(IsReportSafe);

static int CountSafeReportsWithDampener(List<int[]> reports) =>
    reports.Count(IsReportSafeWithDampener);

static bool IsReportSafe(int[] levels)
{
    if (levels.Length < 2)
        return true;

    var differences = levels.Zip(levels.Skip(1), (a, b) => b - a).ToArray();
    
    var isIncreasing = differences.All(d => d > 0);
    var isDecreasing = differences.All(d => d < 0);
    
    if (!isIncreasing && !isDecreasing)
        return false;

    return differences.All(diff => Math.Abs(diff) is >= 1 and <= 3);
}

static bool IsReportSafeWithDampener(int[] levels)
{
    // If it's already safe, no need to try removing levels
    if (IsReportSafe(levels))
        return true;

    // Try removing each level one at a time
    for (int i = 0; i < levels.Length; i++)
    {
        var modifiedLevels = levels.Take(i).Concat(levels.Skip(i + 1)).ToArray();
        if (IsReportSafe(modifiedLevels))
            return true;
    }

    return false;
}

static void AnalyzeReportsInDetail(List<int[]> reports)
{
    Console.WriteLine("\nDetailed Analysis:");
    Console.WriteLine("------------------");

    foreach (var report in reports)
    {
        var isSafeNormal = IsReportSafe(report);
        var isSafeWithDampener = IsReportSafeWithDampener(report);
        var status = GetReportStatus(isSafeNormal, isSafeWithDampener);
        
        Console.WriteLine($"{string.Join(" ", report)}: {status}");

        if (!isSafeNormal && isSafeWithDampener)
        {
            var removableIndex = FindRemovableLevel(report);
            Console.WriteLine($"Can be made safe by removing level at position {removableIndex + 1} (value: {report[removableIndex]})");
        }
        else if (!isSafeWithDampener)
        {
            var reason = GetUnsafeReason(report);
            Console.WriteLine($"Reason: {reason}");
        }
        Console.WriteLine();
    }
}

static string GetReportStatus(bool isSafeNormal, bool isSafeWithDampener)
{
    if (isSafeNormal) return "Safe without dampener";
    if (isSafeWithDampener) return "Safe with dampener";
    return "Unsafe";
}

static int FindRemovableLevel(int[] levels)
{
    for (int i = 0; i < levels.Length; i++)
    {
        var modifiedLevels = levels.Take(i).Concat(levels.Skip(i + 1)).ToArray();
        if (IsReportSafe(modifiedLevels))
            return i;
    }
    return -1;
}

static string GetUnsafeReason(int[] levels)
{
    if (levels.Length < 2)
        return "Report too short";

    var differences = levels.Zip(levels.Skip(1), (a, b) => b - a).ToArray();
    
    if (differences.Any(d => d == 0))
        return "Contains neither increase nor decrease";

    var hasPositive = differences.Any(d => d > 0);
    var hasNegative = differences.Any(d => d < 0);
    if (hasPositive && hasNegative)
        return "Mixed increasing and decreasing";

    var invalidDifference = differences.FirstOrDefault(d => Math.Abs(d) > 3);
    if (invalidDifference != 0)
        return $"Adjacent difference of {Math.Abs(invalidDifference)} is too large";

    return "Unknown reason";
}