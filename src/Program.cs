using System.Text.Json;

// var (command, param) = args.Length switch
// {
//     0 => throw new InvalidOperationException("Usage: your_bittorrent.sh <command> <param>"),
//     1 => throw new InvalidOperationException("Usage: your_bittorrent.sh <command> <param>"),
//     _ => (args[0], args[1])
// };

var programArgs = args
    .Select(x => x.Replace("\"", string.Empty))
    .ToArray();

var command = Array.IndexOf(programArgs, "decode") != -1
    ? "decode"
    : throw new InvalidOperationException("Usage: your_bittorrent.sh decode <param>");

if (command == "decode")
{
    var encodedValue = args[1];
    if (char.IsDigit(encodedValue[0]))
    {
        // Example: "5:hello" -> "hello"
        var colonIndex = encodedValue.IndexOf(':');
        if (colonIndex != -1)
        {
            var strLength = int.Parse(encodedValue[..colonIndex]);
            var strValue = encodedValue.Substring(colonIndex + 1, strLength);
            Console.WriteLine(JsonSerializer.Serialize(strValue));
        }
        else
        {
            throw new InvalidOperationException("Invalid encoded value: " + encodedValue);
        }
    }
    else if (encodedValue[0] == 'i')
    {
        var longValue = long.Parse(encodedValue[1..^1]);
        Console.WriteLine(JsonSerializer.Serialize(longValue));
    }
    // Running ./your_bittorrent.sh decode l5:applei988ee
    // Expected output: ["apple",988]
    else if (encodedValue[0] == 'l')
    {
        var listValue = new List<object>();
        var i = 1;
        while (i < encodedValue.Length - 1)
        {
            var item = encodedValue[i] switch
            {
                'i' => long.Parse(encodedValue[(i + 1)..encodedValue.IndexOf('e', i + 1)]),
                'l' => throw new InvalidOperationException("Nested lists are not supported"),
                'd' => throw new InvalidOperationException("Dictionaries are not supported"),
                _ => throw new InvalidOperationException("Invalid list item")
            };
            listValue.Add(item);
            i = encodedValue.IndexOf('e', i + 1) + 1;
        }
        Console.WriteLine(JsonSerializer.Serialize(listValue));
    }
    else
    {
        throw new InvalidOperationException("Invalid encoded value: " + encodedValue);
    }
}
else
{
    throw new InvalidOperationException($"Invalid command: {command}");
}