using System.Text;

namespace codecrafters_bittorrent.Bencoding;
public static class BencodeEncoder
{
    public static byte[] Encode(object value)
    {
        return value switch
        {
            string str => EncodeString(Encoding.UTF8.GetBytes(str)),
            byte[] bytes => EncodeString(bytes),
            long lng => EncodeInteger(lng),
            int i => EncodeInteger(i),
            List<object> list => EncodeList(list),
            Dictionary<byte[], object> dict => EncodeDictionary(dict),
            _ => throw new ArgumentException($"Invalid value type: {value.GetType()}")
        };
    }

    private static byte[] EncodeString(byte[] value)
    {
        var length = Encoding.UTF8.GetBytes(value.Length.ToString());
        var colon = new[] { BencodingIndicators.SemiColon };
        return Combine(length, colon, value);
    }

    private static byte[] EncodeInteger(long value)
    {
        var prefix = new[] { BencodingIndicators.IntegerIndicator };
        var suffix = new[] { BencodingIndicators.EndIndicator };
        var valueBytes = Encoding.UTF8.GetBytes(value.ToString());
        return Combine(prefix, valueBytes, suffix);
    }

    private static byte[] EncodeList(List<object> value)
    {
        var bytes = new List<byte> { BencodingIndicators.ListIndicator };

        foreach (var item in value)
        {
            bytes.AddRange(Encode(item));
        }

        bytes.Add(BencodingIndicators.EndIndicator);
        return bytes.ToArray();
    }

    public static byte[] EncodeDictionary(Dictionary<byte[], object> value)
    {
        var bytes = new List<byte> { BencodingIndicators.DictionaryIndicator };

        foreach (var key in value.Keys)
        {
            var k = EncodeString(key);
            bytes.AddRange(k);
            bytes.AddRange(Encode(value[key]));
        }

        bytes.Add(BencodingIndicators.EndIndicator);
        return bytes.ToArray();
    }

    private static byte[] Combine(params byte[][] arrays)
    {
        var combined = new byte[arrays.Sum(a => a.Length)];
        var offset = 0;
        foreach (var array in arrays)
        {
            Buffer.BlockCopy(array, 0, combined, offset, array.Length);
            offset += array.Length;
        }

        return combined;
    }
}