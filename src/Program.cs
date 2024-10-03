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
        var encodingTypeValue = Encoding.UTF8.GetBytes(args[1] ?? throw new ArgumentNullException(args[1]));
        var index = 0;
        var result = BencodeDecoder.Decode(ref encodingTypeValue, ref index);

        switch (result)
        {
            case Dictionary<byte[], object> byteDict:
            {
                var stringDict = new Dictionary<string, object>();
                foreach (var kvp in byteDict)
                {
                    var keyString = Encoding.UTF8.GetString(kvp.Key);
                    switch (kvp.Value)
                    {
                        case byte[] valueBytes:
                        {
                            var valueString = Encoding.UTF8.GetString(valueBytes);
                            stringDict[keyString] = valueString;
                            break;
                        }
                        case Dictionary<byte[], object> nestedDict:
                            stringDict[keyString] = DecodeNestedValue(nestedDict);
                            break;
                        case List<object> list:
                            stringDict[keyString] = DecodeNestedValue(list);
                            break;
                        default:
                            stringDict[keyString] = kvp.Value;
                            break;
                    }
                }

                result = stringDict;
                break;
            }
            case List<object> list:
                result = DecodeNestedValue(list);
                break;
            case byte[] byteArray:
                result = Encoding.UTF8.GetString(byteArray);
                break;
        }

        Console.WriteLine(JsonSerializer.Serialize(result));

        break;
    }
    case "info":
    {
        var torrentFileName = args[1] ?? throw new ArgumentNullException(args[1]);

        var torrentInfoInBytes = File.ReadAllBytes(torrentFileName);

        var index = 0;
        var result = BencodeDecoder.DecodeDictionary(ref torrentInfoInBytes, ref index);
        var infoDictionary = (Dictionary<byte[], object>)result["info"u8.ToArray()];

        infoDictionary.TryGetValue("piece length"u8.ToArray(), out var pieceLength);
        infoDictionary.TryGetValue("pieces"u8.ToArray(), out var piecesValue);

        var pieces = (byte[])piecesValue!;
        var bencodedInfo = BencodeEncoder.EncodeDictionary(infoDictionary);
        var infoHashString = CalculateInfoHash(bencodedInfo);

        var trackerUrl = Encoding.UTF8.GetString((byte[])result["announce"u8.ToArray()]);
        var trackerUrlMessage = $"Tracker URL: {trackerUrl}";
        var lengthMessage = $"Length: {((Dictionary<byte[], object>)result["info"u8.ToArray()])["length"u8.ToArray()]}";
        var infoHashMessage = $"Info Hash: {infoHashString}";
        var pieceLengthMessage = $"Piece Length: {pieceLength}";

        Console.WriteLine(trackerUrlMessage);
        Console.WriteLine(lengthMessage);
        Console.WriteLine(infoHashMessage);
        Console.WriteLine(pieceLengthMessage);
        Console.WriteLine("Piece Hashes:");
        foreach (var hashChunk in pieces.Chunk(20))
        {
            Console.WriteLine(BitConverter.ToString(hashChunk).Replace("-", "").ToLower());
        }

        break;
    }
    case "peers":
        var torrentBytes = File.ReadAllBytes(args[1] ?? throw new ArgumentNullException(args[1]));
        var torrent = new Torrent(torrentBytes);
        break;
    default:
        throw new InvalidOperationException($"Invalid command: {command}");
}

return;

static object DecodeNestedValue(object value)
{
    switch (value)
    {
        case byte[] valueBytes:
            return Encoding.UTF8.GetString(valueBytes);
        case Dictionary<byte[], object> nestedDict:
        {
            var nestedDictString = new Dictionary<string, object>();
            foreach (var nestedKvp in nestedDict)
            {
                var nestedKeyString = Encoding.UTF8.GetString(nestedKvp.Key);
                nestedDictString[nestedKeyString] = DecodeNestedValue(nestedKvp.Value);
            }

            return nestedDictString;
        }
        case List<object> list:
        {
            var listString = new List<object>();
            foreach (var item in list)
            {
                listString.Add(DecodeNestedValue(item));
            }

            return listString;
        }
        default:
            return value;
    }
}

string CalculateInfoHash(byte[] bencodedBytes)
{
    var infoHash = SHA1.HashData(bencodedBytes);

    var infoHashString = BitConverter.ToString(infoHash)
        .Replace("-", "").ToLower();

    return infoHashString;
}