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
    // parse integer.
    // Integers are encoded as i<number>e. For example, 52 is encoded as i52e and -52 is encoded as i-52e.
    else if (encodedValue[0] == 'i')
    {
        var intValue = int.Parse(encodedValue[1..^1]);
        Console.WriteLine(JsonSerializer.Serialize(intValue));
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