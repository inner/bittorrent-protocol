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
                    if (kvp.Value is byte[] valueBytes)
                    {
                        // Convert byte array to UTF-8 string directly
                        var valueString = Encoding.UTF8.GetString(valueBytes);
                        stringDict[keyString] = valueString;
                    }
                    else if (kvp.Value is Dictionary<byte[], object> nestedDict)
                    {
                        // Handle nested dictionaries
                        stringDict[keyString] = DecodeNestedValue(nestedDict);
                    }
                    else if (kvp.Value is List<object> list)
                    {
                        // Handle lists
                        stringDict[keyString] = DecodeNestedValue(list);
                    }
                    else
                    {
                        stringDict[keyString] = kvp.Value;
                    }
                }

                result = stringDict;
                break;
            }
            case List<object> list:
                // Handle lists directly
                result = DecodeNestedValue(list);
                break;
            case byte[] byteArray:
                // Handle plain byte arrays as UTF-8 strings
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
        var bencodedInfo = BencodeEncoder.EncodeDictionary(infoDictionary);
        var infoHashString = CalculateInfoHash(bencodedInfo);

        var trackerUrlMessage = $"Tracker URL: {Encoding.UTF8.GetString((byte[])result["announce"u8.ToArray()])}";
        var lengthMessage = $"Length: {((Dictionary<byte[], object>)result["info"u8.ToArray()])["length"u8.ToArray()]}";
        var infoHashMessage = $"Info Hash: {infoHashString}";

        Console.WriteLine(trackerUrlMessage);
        Console.WriteLine(lengthMessage);
        Console.WriteLine(infoHashMessage);

        break;
    }
    default:
        throw new InvalidOperationException($"Invalid command: {command}");
}

return;

static object DecodeNestedValue(object value)
{
    if (value is byte[] valueBytes)
    {
        // Convert byte array to UTF-8 string
        return Encoding.UTF8.GetString(valueBytes);
    }

    if (value is Dictionary<byte[], object> nestedDict)
    {
        var nestedDictString = new Dictionary<string, object>();
        foreach (var nestedKvp in nestedDict)
        {
            string nestedKeyString = Encoding.UTF8.GetString(nestedKvp.Key);
            nestedDictString[nestedKeyString] = DecodeNestedValue(nestedKvp.Value);
        }

        return nestedDictString;
    }

    if (value is List<object> list)
    {
        var listString = new List<object>();
        foreach (var item in list)
        {
            listString.Add(DecodeNestedValue(item));
        }

        return listString;
    }

    return value;
}

string CalculateInfoHash(byte[] bencodedBytes)
{
    var infoHash = SHA1.HashData(bencodedBytes);

    var infoHashString = BitConverter.ToString(infoHash)
        .Replace("-", "").ToLower();

    return infoHashString;
}