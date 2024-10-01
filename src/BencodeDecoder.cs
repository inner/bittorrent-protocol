using System.Text;

namespace codecrafters_bittorrent;

public static class BencodeDecoder
{
    public static object Decode(ref byte[] encodedValue, ref int index)
    {
        if (encodedValue == null || encodedValue.Length == 0)
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

    private static byte[] DecodeString(ref byte[] encodedValue, ref int index)
    {
        var colonIndex = Array.IndexOf(encodedValue, (byte)':', index);
        var length = int.Parse(Encoding.ASCII.GetString(encodedValue, index, colonIndex - index));
        index = colonIndex + 1;
        var value = new byte[length];
        Array.Copy(encodedValue, index, value, 0, length);
        index += length;
        return value;
    }

    private static object DecodeInteger(ref byte[] encodedValue, ref int index)
    {
        index++;
        var end = Array.IndexOf(encodedValue, (byte)'e', index);
        var value = long.Parse(Encoding.ASCII.GetString(encodedValue, index, end - index));
        index = end + 1;
        return value;
    }

    private static object DecodeList(ref byte[] encodedValue, ref int index)
    {
        var list = new List<object>();
        index++;

        while (encodedValue[index] != (byte)'e')
        {
            list.Add(Decode(ref encodedValue, ref index));
        }

        index++;
        return list;
    }

    public static Dictionary<byte[], object> DecodeDictionary(ref byte[] encodedValue, ref int index)
    {
        var dictionary = new Dictionary<byte[], object>(new ByteArrayEqualityComparer());
        index++;

        while (encodedValue[index] != (byte)'e')
        {
            var key = DecodeString(ref encodedValue, ref index);
            var value = Decode(ref encodedValue, ref index);
            dictionary.Add(key, value);
        }

        index++;
        return dictionary;
    }

    private class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[]? x, byte[]? y)
        {
            if (x == null || y == null)
                return x == y;
            if (x.Length != y.Length)
                return false;
            for (var i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                    return false;
            }

            return true;
        }

        public int GetHashCode(byte[]? obj)
        {
            if (obj == null)
                return 0;
            unchecked
            {
                var hash = 17;
                foreach (var b in obj)
                {
                    hash = hash * 31 + b;
                }

                return hash;
            }
        }
    }
}