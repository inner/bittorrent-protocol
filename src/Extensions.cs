using System.Text;

namespace codecrafters_bittorrent;

public static class Extensions
{
    public static BencodingType ToBencodingType(this byte[] encodedValue, ref int index)
    {
        if (encodedValue == null || encodedValue.Length == 0)
            throw new ArgumentNullException(nameof(encodedValue));

        if (char.IsDigit((char)encodedValue[index]))
            return BencodingType.String;

        return encodedValue[index] switch
        {
            (byte)'i' => BencodingType.Integer,
            (byte)'l' => BencodingType.List,
            (byte)'d' => BencodingType.Dictionary,
            _ => throw new InvalidOperationException("Invalid encoded value: " + Encoding.UTF8.GetString(encodedValue))
        };
    }
}