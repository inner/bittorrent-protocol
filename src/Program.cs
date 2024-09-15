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
    else if (encodedValue[0] == 'l')
    {
        var listValue = new List<object>();
        var i = 1;
        while (i < encodedValue.Length - 1)
        {
            var element = encodedValue[i];
            if (char.IsDigit(element))
            {
                var colonIndex = encodedValue.IndexOf(':', i);
                if (colonIndex != -1)
                {
                    var strLength = int.Parse(encodedValue[i..colonIndex]);
                    var strValue = encodedValue.Substring(colonIndex + 1, strLength);
                    listValue.Add(strValue);
                    i = colonIndex + 1 + strLength;
                }
                else
                {
                    throw new InvalidOperationException("Invalid encoded value: " + encodedValue);
                }
            }
            else if (element == 'i')
            {
                var endIndex = encodedValue.IndexOf('e', i);
                var longValue = long.Parse(encodedValue[i..endIndex]);
                listValue.Add(longValue);
                i = endIndex + 1;
            }
            else if (element == 'l')
            {
                var endIndex = encodedValue.IndexOf('e', i);
                var listElement = encodedValue[i..(endIndex + 1)];
                listValue.Add(listElement);
                i = endIndex + 1;
            }
            else
            {
                throw new InvalidOperationException("Invalid encoded value: " + encodedValue);
            }
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