namespace codecrafters_bittorrent;

public static class BencodeDecoder
{
    public static object Decode(ref string encodedValue, ref int index)
    {
        if (string.IsNullOrWhiteSpace(encodedValue))
            throw new ArgumentNullException(nameof(encodedValue));
        
        var bencodingType = encodedValue.ToBencodingType(ref index);
        var decodedValue = bencodingType switch
        {
            BencodingType.String => DecodeString(ref encodedValue, ref index),
            BencodingType.Integer => DecodeInteger(ref encodedValue, ref index),
            BencodingType.List => DecodeList(ref encodedValue, ref index),
            BencodingType.Dictionary => DecodeDictionary(ref encodedValue, ref index),
            _ => throw new ArgumentException("Invalid bencoding type: " + bencodingType)
        };

        return decodedValue;
    }

    private static object DecodeString(ref string encodedValue, ref int index)
    {
        var colonIndex = encodedValue.IndexOf(':', index);
        var length = int.Parse(encodedValue.Substring(index, colonIndex - index));
        index = colonIndex + 1;
        var value = encodedValue.Substring(index, length);
        index += length;
        return value;
    }

    private static object DecodeInteger(ref string encodedValue, ref int index)
    {
        index++;
        var end = encodedValue.IndexOf('e', index);
        var value = long.Parse(encodedValue.Substring(index, end - index));
        index = end + 1;
        return value;
    }

    private static object DecodeList(ref string encodedValue, ref int index)
    {
        var list = new List<object>();
        index++;

        while (encodedValue[index] != 'e')
        {
            list.Add(Decode(ref encodedValue, ref index));
        }

        index++;
        return list;
    }

    public static Dictionary<string, object> DecodeDictionary(ref string encodedValue, ref int index)
    {
        var dictionary = new Dictionary<string, object>();
        index++;

        while (encodedValue[index] != 'e')
        {
            var key = DecodeString(ref encodedValue, ref index);
            var value = Decode(ref encodedValue, ref index);
            dictionary.Add((string)key, value);
        }

        index++;
        return dictionary;
    }
}