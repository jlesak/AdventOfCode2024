using System.Text.RegularExpressions;

const string FileName = "Input.txt";

var fileStream = File.OpenRead(FileName);
var reader = new StreamReader(fileStream);
var memoryContent = await reader.ReadToEndAsync();

var part1Result = RegexPart1(memoryContent);
var part2Result = RegexPart2(memoryContent);

Console.WriteLine($"Part1 Result: {part1Result}");
Console.WriteLine($"Part2 Result: {part2Result}");

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

static int RegexPart2(string inputContent)
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