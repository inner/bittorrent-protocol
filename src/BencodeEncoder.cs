using System.Text;

namespace codecrafters_bittorrent;
public static class BencodeEncoder
{
    public static byte[] Encode(object value)
    {
        return value switch
        {
            string str => EncodeString(Encoding.UTF8.GetBytes(str)),
            byte[] bytes => EncodeString(bytes),
            long lng => EncodeInteger(lng),
            List<object> list => EncodeList(list),
            Dictionary<byte[], object> dict => EncodeDictionary(dict),
            _ => throw new ArgumentException($"Invalid value type: {value.GetType()}")
        };
    }

    private static byte[] EncodeString(byte[] value)
    {
        var length = Encoding.UTF8.GetBytes(value.Length.ToString());
        var colon = new[] { (byte)':' };
        return Combine(length, colon, value);
    }

    private static byte[] EncodeInteger(long value)
    {
        var prefix = new[] { (byte)'i' };
        var suffix = new[] { (byte)'e' };
        var valueBytes = Encoding.UTF8.GetBytes(value.ToString());
        return Combine(prefix, valueBytes, suffix);
    }

    private static byte[] EncodeList(List<object> value)
    {
        var bytes = new List<byte> { (byte)'l' };

        foreach (var item in value)
        {
            bytes.AddRange(Encode(item));
        }

        bytes.Add((byte)'e');
        return bytes.ToArray();
    }

    public static byte[] EncodeDictionary(Dictionary<byte[], object> value)
    {
        var bytes = new List<byte> { (byte)'d' };
        
        foreach (var key in value.Keys)
        {
            var k = EncodeString(key);
            bytes.AddRange(k);
            bytes.AddRange(Encode(value[key]));
        }

        bytes.Add((byte)'e');
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