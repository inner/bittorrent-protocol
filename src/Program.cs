using System.Text;
using System.Text.Json;
using Decoder = codecrafters_bittorrent.Decoder;

var programArgs = args
    .Select(x => x.Replace("\"", string.Empty))
    .ToArray();

var command = programArgs[0] ?? throw new ArgumentNullException(programArgs[0]);

if (command == "decode")
{
    var encodingTypeValue = args[1] ?? throw new ArgumentNullException(args[1]);
    var index = 0;
    var decodedResult = Decoder.Decode(ref encodingTypeValue, ref index);
    Console.WriteLine(JsonSerializer.Serialize(decodedResult));
}
else if (command == "info")
{
    var torrentFileName = args[1] ?? throw new ArgumentNullException(args[1]);
    var torrentInfo = File.ReadAllText(torrentFileName, Encoding.ASCII);
    var index = 0;
    var result = Decoder.DecodeDictionary(ref torrentInfo, ref index);
    Console.WriteLine($"Tracker URL: {result["announce"]}");
    Console.WriteLine($"Length: {((Dictionary<string, object>)result["info"])["length"]}");
}
else
{
    throw new InvalidOperationException($"Invalid command: {command}");
}