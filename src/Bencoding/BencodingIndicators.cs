namespace codecrafters_bittorrent.Bencoding;

public static class BencodingIndicators
{
    public const byte IntegerIndicator = (byte)'i';
    public const byte ListIndicator = (byte)'l';
    public const byte DictionaryIndicator = (byte)'d';
    public const byte EndIndicator = (byte)'e';
}