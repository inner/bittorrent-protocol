using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using codecrafters_bittorrent;

var programArgs = args
    .Select(x => x.Replace("\"", string.Empty))
    .ToArray();

var command = programArgs[0] ?? throw new ArgumentNullException(programArgs[0]);

switch (command)
{
    case "decode":
    {
        var encodingTypeValue = args[1] ?? throw new ArgumentNullException(args[1]);
        var index = 0;
        var decodedResult = BencodeDecoder.Decode(ref encodingTypeValue, ref index);
        Console.WriteLine(JsonSerializer.Serialize(decodedResult));
        break;
    }
    case "info":
    {
        var torrentFileName = args[1] ?? throw new ArgumentNullException(args[1]);
        var torrentInfo = File.ReadAllText(torrentFileName, Encoding.ASCII);
        var index = 0;
        var result = BencodeDecoder.DecodeDictionary(ref torrentInfo, ref index);
        var infoDictionary = (Dictionary<string, object>)result["info"];
        var bencodedInfo = BencodeEncoder.EncodeDictionary(infoDictionary);
        var infoHashString = CalculateInfoHash(bencodedInfo);
        
        Console.WriteLine($"Tracker URL: {result["announce"]}");
        Console.WriteLine($"Length: {((Dictionary<string, object>)result["info"])["length"]}");
        Console.WriteLine($"Info Hash: {infoHashString}");
        break;
    }
    default:
        throw new InvalidOperationException($"Invalid command: {command}");
}

return;

string CalculateInfoHash(string bencodedString)
{
    var bytes = Encoding.ASCII.GetBytes(bencodedString);

    var cleanedBytes = bytes
        .Where(b => b != 0x00).ToArray();

    var infoHash = SHA1.HashData(cleanedBytes);
    var infoHashString = BitConverter.ToString(infoHash)
        .Replace("-", "").ToLower();

    return infoHashString;
}