namespace codecrafters_bittorrent;

public static class Extensions
{
    public static BencodingType ToBencodingType(this string encodedValue)
    {
        if (string.IsNullOrWhiteSpace(encodedValue))
            throw new ArgumentNullException(nameof(encodedValue));

        if (char.IsDigit(encodedValue[0]))
            return BencodingType.String;

        return encodedValue[0] switch
        {
            'i' => BencodingType.Integer,
            'l' => BencodingType.List,
            'd' => BencodingType.Dictionary,
            _ => throw new InvalidOperationException("Invalid encoded value: " + encodedValue)
        };
    }
}