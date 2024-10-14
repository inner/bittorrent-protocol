using System.Text;
using System.Text.Json;
using codecrafters_bittorrent.Bencoding;

namespace codecrafters_bittorrent.Commands;

public class Decode : IBCommand
{
    public Task Execute(string[] args)
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
        return Task.CompletedTask;
    }
    
    private static object DecodeNestedValue(object value)
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
}