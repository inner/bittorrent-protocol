namespace codecrafters_bittorrent;

public static class Extensions
{
    public static BencodingType ToBencodingType(this string encodedValue, ref int index)
    {
        if (string.IsNullOrWhiteSpace(encodedValue))
            throw new ArgumentNullException(nameof(encodedValue));

        if (char.IsDigit(encodedValue[index]))
            return BencodingType.String;

        return encodedValue[index] switch
        {
            'i' => BencodingType.Integer,
            'l' => BencodingType.List,
            'd' => BencodingType.Dictionary,
            _ => throw new InvalidOperationException("Invalid encoded value: " + encodedValue)
        };
    }
}