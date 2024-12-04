using System.Text.RegularExpressions;
using _3MullItOver;
using BenchmarkDotNet.Running;

const string FileName = "Input.txt";

// var benchmark = BenchmarkRunner.Run<MemoryProcessingBenchmarks>();

var fileStream = File.OpenRead(FileName);
var reader = new StreamReader(fileStream);
var memoryContent = await reader.ReadToEndAsync();

var part1Result = RegexPart1(memoryContent);

var part2ResultOriginal = RegexPart2(memoryContent);
var part2ResultNew = ProcessMemoryPart2Optimized(memoryContent);

Console.WriteLine($"Part1 Result: {part1Result}");
Console.WriteLine($"Part2 Result: {part2ResultNew}");

return;

static int RegexPart1(string inputContent)
{
    const string MultiplicationRegex = @"mul\((\d+),(\d+)\)";
    var regex = new Regex(MultiplicationRegex);

    var totalSum = 0;
    
    foreach (Match match in regex.Matches(inputContent))
    {
        totalSum += int.Parse(match.Groups[1].Value) * int.Parse(match.Groups[2].Value);
    }

    return totalSum;
}

int RegexPart2(string inputContent)
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

int ProcessMemoryPart2Optimized(string input)
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

static (bool isValid, int product, int length) TryParseMultiplication(ReadOnlySpan<char> span, out int consumedLength)
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

static int ParseIntSpan(ReadOnlySpan<char> span)
{
    var result = 0;
    foreach (var c in span)
    {
        result = result * 10 + (c - '0');
    }
    return result;
}
