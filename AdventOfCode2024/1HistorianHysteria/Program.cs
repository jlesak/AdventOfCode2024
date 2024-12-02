// See https://aka.ms/new-console-template for more information

string fileName = "Input.txt";

try
{
    var (leftList, rightList) = ReadInputFile(fileName);
            
    // Part 1
    long totalDistance = CalculateTotalDistance(leftList, rightList);
    Console.WriteLine($"\nPart 1 - Total distance between the lists: {totalDistance:N0}");
            
    // Part 2
    long similarityScore = CalculateSimilarityScore(leftList, rightList);
    Console.WriteLine($"\nPart 2 - Similarity score: {similarityScore:N0}");
}
catch (FileNotFoundException)
{
    Console.WriteLine($"Error: {fileName} file not found!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error occurred: {ex.Message}");
}

return;

static (List<long> leftList, List<long> rightList) ReadInputFile(string filePath)
{
    var leftList = new List<long>();
    var rightList = new List<long>();

    foreach (string line in File.ReadLines(filePath))
    {
        if (string.IsNullOrWhiteSpace(line)) continue;

        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            if (long.TryParse(parts[0], out long leftNum))
                leftList.Add(leftNum);
            if (long.TryParse(parts[1], out long rightNum))
                rightList.Add(rightNum);
        }
        else
        {
            Console.WriteLine($"Warning: Skipping invalid line: {line}");
        }
    }

    if (leftList.Count != rightList.Count)
    {
        throw new InvalidDataException("Left and right lists have different lengths!");
    }

    return (leftList, rightList);
}

static long CalculateTotalDistance(List<long> leftList, List<long> rightList)
{
    var sortedLeft = new List<long>(leftList);
    var sortedRight = new List<long>(rightList);
    
    // Sort both lists
    sortedLeft.Sort();
    sortedRight.Sort();

    long totalDistance = 0;

    // Calculate distances for each pair
    for (int i = 0; i < sortedLeft.Count; i++)
    {
        long distance = Math.Abs(sortedLeft[i] - sortedRight[i]);
        totalDistance += distance;
    }

    return totalDistance;
}

static long CalculateSimilarityScore(List<long> leftList, List<long> rightList)
{
    // Create frequency dictionary for right list
    var rightFrequency = new Dictionary<long, int>();
    foreach (var num in rightList)
    {
        if (!rightFrequency.ContainsKey(num))
            rightFrequency[num] = 0;
        rightFrequency[num]++;
    }

    long totalScore = 0;

    // Process each number in left list
    for (int i = 0; i < leftList.Count; i++)
    {
        long leftNum = leftList[i];
        int frequency = rightFrequency.GetValueOrDefault(leftNum, 0);
        long score = leftNum * frequency;
        
        Console.WriteLine($"Number {leftNum,8:N0} appears {frequency,2} times in right list. Score: {leftNum,8:N0} * {frequency} = {score,10:N0}");
        
        totalScore += score;
    }

    return totalScore;
}