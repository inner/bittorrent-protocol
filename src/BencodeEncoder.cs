using System.Text;

namespace codecrafters_bittorrent;

public static class BencodeEncoder
{
    public static string Encode(object value)
    {
        return value switch
        {
            string stringValue => EncodeString(stringValue),
            long longValue => EncodeInteger(longValue),
            int intValue => EncodeInteger(intValue),
            List<object> listValue => EncodeList(listValue),
            Dictionary<string, object> dictionaryValue => EncodeDictionary(dictionaryValue),
            _ => throw new ArgumentException("Invalid value type: " + value.GetType())
        };
    }

    private static string EncodeString(string value)
    {
        return $"{value.Length}:{value}";
    }

    private static string EncodeInteger(long value)
    {
        return $"i{value}e";
    }

    private static string EncodeList(List<object> value)
    {
        var sb = new StringBuilder();
        sb.Append('l');

        foreach (var item in value)
        {
            sb.Append(Encode(item));
        }

        sb.Append('e');
        return sb.ToString();
    }

    public static string EncodeDictionary(Dictionary<string, object> value)
    {
        var sb = new StringBuilder();
        sb.Append('d');

        foreach (var key in value.Keys)
        {
            sb.Append(EncodeString(key));
            sb.Append(Encode(value[key]));
        }

        sb.Append('e');
        return sb.ToString();
    }
}