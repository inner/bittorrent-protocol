namespace codecrafters_bittorrent;

public static class Decoder
{
    public static object Decode(string encodedValue)
    {
        if (string.IsNullOrWhiteSpace(encodedValue))
            throw new ArgumentNullException(nameof(encodedValue));

        var bencodingType = encodedValue.ToBencodingType();
        var decoderAction = Map[bencodingType];
        return decoderAction.Invoke(encodedValue);
    }

    private static readonly Dictionary<BencodingType, Func<string, object>> Map = new()
    {
        { BencodingType.String, DecodeString },
        { BencodingType.Integer, DecodeInteger },
        { BencodingType.List, DecodeList },
        { BencodingType.Dictionary, DecodeDictionary }
    };

    private static object DecodeString(string encodedValue)
    {
        var colonIndex = encodedValue.IndexOf(':');
        if (colonIndex == -1)
            ThrowInvalidEncodedValue(encodedValue);

        var strLength = int.Parse(encodedValue[..colonIndex]);
        return encodedValue.Substring(colonIndex + 1, strLength);
    }

    private static object DecodeInteger(string encodedValue)
    {
        var endIndex = encodedValue.IndexOf('e');
        if (endIndex == -1)
            ThrowInvalidEncodedValue(encodedValue);

        return long.Parse(encodedValue[1..endIndex]);
    }

    private static object DecodeList(string encodedValue)
    {
        var list = new List<object>();

        var listContents = encodedValue.AsSpan(1, encodedValue.Length - 2);
        if (listContents.IsEmpty)
            return list;

        while (listContents.Length > 0)
        {
            var bencodingType = listContents.ToString().ToBencodingType();
            var decoderAction = Map[bencodingType];
            var decodedValue = decoderAction.Invoke(listContents.ToString());
            list.Add(decodedValue);

            switch (bencodingType)
            {
                case BencodingType.String:
                {
                    var lenToRemove = decodedValue.ToString()!.Length + 2;
                    listContents = listContents.Slice(lenToRemove);
                    break;
                }
                case BencodingType.Integer:
                {
                    var lenToRemove = decodedValue.ToString()!.Length + 2;
                    listContents = listContents.Slice(lenToRemove);
                    break;
                }
                // handle this:
                // decode lli105e5:mangoee
                default:
                    throw new ArgumentException("Invalid bencoding type: " + bencodingType);
            }
        }

        return list;
    }

    private static object DecodeDictionary(string encodedValue)
    {
        var dictionary = new Dictionary<string, object>();
        var index = 1;
        while (index < encodedValue.Length - 1)
        {
            var key = encodedValue[index..];
            var keyBencodingType = key.ToBencodingType();
            if (keyBencodingType != BencodingType.String)
                ThrowInvalidEncodedValue(key);

            var keyDecoderAction = Map[keyBencodingType];
            var decodedKey = (string)keyDecoderAction.Invoke(key);
            index += key.Length;

            var value = encodedValue[index..];
            var valueBencodingType = value.ToBencodingType();
            var valueDecoderAction = Map[valueBencodingType];
            var decodedValue = valueDecoderAction.Invoke(value);
            dictionary.Add(decodedKey, decodedValue);
            index += value.Length;
        }

        return dictionary;
    }

    private static void ThrowInvalidEncodedValue(string encodedValue)
    {
        throw new InvalidOperationException("Invalid encoded value: " + encodedValue);
    }
}