using System.Text.Json;
using codecrafters_bittorrent;

var programArgs = args
    .Select(x => x.Replace("\"", string.Empty))
    .ToArray();

var command = Array.IndexOf(programArgs, "decode") != -1
    ? "decode"
    : throw new InvalidOperationException("Usage: your_bittorrent.sh decode <param>");

if (command == "decode")
{
    var encodingTypeValue = args[1] ?? throw new ArgumentNullException(args[1]);
    var decodedResult = Decoder.Decode(encodingTypeValue);
    Console.WriteLine(JsonSerializer.Serialize(decodedResult));
}
else
{
    throw new InvalidOperationException($"Invalid command: {command}");
}