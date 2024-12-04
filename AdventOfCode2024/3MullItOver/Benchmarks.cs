using BenchmarkDotNet.Attributes;
using System.Text.RegularExpressions;

namespace _3MullItOver;

[MemoryDiagnoser]
public class MemoryProcessingBenchmarks
{
    private string input = string.Empty;

    private const int Part1Result = 175700056;
    private const int Part2Result = 71668682;
    
    // Cached regex patterns for optimized version
    private static readonly Regex MultiplicationRegex = new(@"mul\((\d{1,3}),(\d{1,3})\)", RegexOptions.Compiled);
    private static readonly Regex DoStatement = new(@"do\(\)", RegexOptions.Compiled);
    private static readonly Regex DontStatement = new(@"don't\(\)", RegexOptions.Compiled);
    
    [GlobalSetup]
    public void Setup()
    {
        input = File.ReadAllText("Input.txt");
    }

    [Benchmark(Baseline = true)]
    public (int part1, int part2) OriginalSolution()
    {
        var part1 = ProcessMemoryPart1Original(input);
        var part2 = ProcessMemoryPart2Original(input);
        if (part1 != Part1Result || part2 != Part2Result)
        {
            throw new Exception($"Results are incorrect: {part1} != {Part1Result} or {part2} != {Part2Result}");
        }
        return (part1, part2);
    }
    
    [Benchmark]
    public (int part1, int part2) OptimizedRegex()
    {
        var part1 = ProcessMemoryPart1OptimizedRegex(input);
        var part2 = ProcessMemoryPart2OptimizedRegex(input);
        if (part1 != Part1Result || part2 != Part2Result)
        {
            throw new Exception($"Results are incorrect: {part1} != {Part1Result} or {part2} != {Part2Result}");
        }
        return (part1, part2);
    }

    [Benchmark]
    public (int part1, int part2) OptimizedSolution()
    {
        var part1 = ProcessMemoryPart1Optimized(input);
        var part2 = ProcessMemoryPart2Optimized(input);
        
        if (part1 != Part1Result || part2 != Part2Result)
        {
            throw new Exception($"Results are incorrect: {part1} != {Part1Result} or {part2} != {Part2Result}");
        }
        
        return (part1, part2);
    }

    // Original methods (as in previous solution)
    private static int ProcessMemoryPart1Original(string input) 
    {
        const string MultiplicationRegex = @"mul\((\d+),(\d+)\)";
        var regex = new Regex(MultiplicationRegex);

        var totalSum = 0;
    
        foreach (Match match in regex.Matches(input))
        {
            totalSum += int.Parse(match.Groups[1].Value) * int.Parse(match.Groups[2].Value);
        }

        return totalSum;
    }

    private static int ProcessMemoryPart2Original(string inputContent)
    {
        const string DoStatement = "do()";
        const string DoStatementRegex = @"do\(\)";
    
        const string DontStatement = "don't()";
        const string DontStatementRegex = @"don't\(\)";
    
        const string MultiplicationRegex = @"mul\((\d+),(\d+)\)";
    
        var regex = new Regex(MultiplicationRegex);
        var matchedStatements = regex.Matches(inputContent).ToList();
    
        regex = new Regex(DoStatementRegex);
        matchedStatements.AddRange(regex.Matches(inputContent));
    
        regex = new Regex(DontStatementRegex);
        matchedStatements.AddRange(regex.Matches(inputContent));

        var doMultiplication = true;
        var totalSum = 0;

        foreach (var match in matchedStatements.OrderBy(x => x.Index))
        {
            switch (match.Value)
            {
                case DontStatement:
                    doMultiplication = false;
                    continue;
                case DoStatement:
                    doMultiplication = true;
                    continue;
            }

            if (!doMultiplication)
            {
                continue;
            }
        
            totalSum += int.Parse(match.Groups[1].Value) * int.Parse(match.Groups[2].Value);
        }
    
        return totalSum;
    }
    
    // Optimized methods with regex
    private static int ProcessMemoryPart1OptimizedRegex(string input) 
    {
        var totalSum = 0;
    
        foreach (Match match in MultiplicationRegex.Matches(input))
        {
            if (int.TryParse(match.Groups[1].Value, out var x) && 
                int.TryParse(match.Groups[2].Value, out var y))
            {
                totalSum += x * y;
            }
        }

        return totalSum;
    }

    private static int ProcessMemoryPart2OptimizedRegex(string inputContent)
    {
        var matchedStatements = MultiplicationRegex.Matches(inputContent).ToList();
        matchedStatements.AddRange(MemoryProcessingBenchmarks.DoStatement.Matches(inputContent));
        matchedStatements.AddRange(MemoryProcessingBenchmarks.DontStatement.Matches(inputContent));

        var doMultiplication = true;
        var totalSum = 0;

        foreach (var match in matchedStatements.OrderBy(x => x.Index).ToArray())
        {
            switch (match.Value)
            {
                case "don't()":
                    doMultiplication = false;
                    continue;
                case "do()":
                    doMultiplication = true;
                    continue;
            }
            
            if (doMultiplication &&
                int.TryParse(match.Groups[1].Value, out var x) && 
                int.TryParse(match.Groups[2].Value, out var y))
            {
                totalSum += x * y;
            }
        }
    
        return totalSum;
    }

    // Optimized methods
    private static int ProcessMemoryPart1Optimized(string input)
    {
        var sum = 0;
        var matches = MultiplicationRegex.Matches(input);
        
        foreach (Match match in matches)
        {
            if (match.Groups.Count == 3 && 
                int.TryParse(match.Groups[1].Value, out int x) && 
                int.TryParse(match.Groups[2].Value, out int y))
            {
                sum += x * y;
            }
        }
        
        return sum;
    }

    private static int ProcessMemoryPart2Optimized(string input)
{
    var sum = 0;
    var span = input.AsSpan();
    var isEnabled = true;
    var position = 0;
    
    while (position < span.Length)
    {
        if (span[position..].StartsWith("do()"))
        {
            isEnabled = true;
            position += 4;
            continue;
        }
            
        if (span[position..].StartsWith("don't()"))
        {
            isEnabled = false;
            position += 7;
            continue;
        }
            
        if (isEnabled && span[position..].StartsWith("mul("))
        {
            var result = TryParseMultiplication(span[position..], out var multiplierSpan);
            if (result.isValid)
            {
                sum += result.product;
                position += multiplierSpan;
                continue;
            }
        }
            
        position++;
    }
    
    return sum;
}

    private static (bool isValid, int product, int length) TryParseMultiplication(ReadOnlySpan<char> span, out int consumedLength)
{
    consumedLength = 0;
    // Check minimum length for "mul(x,y)"
    if (span.Length < 7) return (false, 0, 0);

    // Skip "mul("
    var pos = 4;
        
    // Parse first number
    var firstNumberStart = pos;
    while (pos < span.Length && char.IsDigit(span[pos])) pos++;
        
    if (pos == firstNumberStart || pos - firstNumberStart > 3) 
        return (false, 0, 0);

    if (pos >= span.Length || span[pos] != ',') 
        return (false, 0, 0);

    var firstNumber = ParseIntSpan(span.Slice(firstNumberStart, pos - firstNumberStart));
        
    // Skip comma
    pos++;
        
    // Parse second number
    var secondNumberStart = pos;
    while (pos < span.Length && char.IsDigit(span[pos])) pos++;
        
    if (pos == secondNumberStart || pos - secondNumberStart > 3) 
        return (false, 0, 0);

    if (pos >= span.Length || span[pos] != ')') 
        return (false, 0, 0);

    var secondNumber = ParseIntSpan(span.Slice(secondNumberStart, pos - secondNumberStart));
        
    // Include closing parenthesis in consumed length
    consumedLength = pos + 1;
    return (true, firstNumber * secondNumber, pos + 1);
}

    private static int ParseIntSpan(ReadOnlySpan<char> span)
{
    var result = 0;
    foreach (var c in span)
    {
        result = result * 10 + (c - '0');
    }
    return result;
}
}

