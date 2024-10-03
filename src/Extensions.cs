using System.Security.Cryptography;
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
    
    public static Dictionary<byte[], object> ToDictionary(this byte[] torrentBytes)
    {
        var index = 0;
        var torrentBytesDictionary = BencodeDecoder.DecodeDictionary(ref torrentBytes, ref index);
        return torrentBytesDictionary;
    }

    public static Dictionary<byte[], object> ToInfoDictionary(this Dictionary<byte[], object> torrentDictionary)
    {
        var infoDictionary = (Dictionary<byte[], object>)torrentDictionary["info"u8.ToArray()];
        return infoDictionary;
    }
    
    public static byte[] ToPiecesInBytes(this Dictionary<byte[], object> infoDictionary)
    {
        var piecesInBytes = (byte[])infoDictionary["pieces"u8.ToArray()];
        return piecesInBytes;
    }
    
    public static string ToInfoHash(this Dictionary<byte[], object> infoDictionary)
    {
        return BencodeEncoder.EncodeDictionary(infoDictionary).CalculateInfoHash();
    }
    
    public static string ToTrackerUrl(this Dictionary<byte[], object> torrentDictionary)
    {
        return Encoding.UTF8.GetString((byte[])torrentDictionary["announce"u8.ToArray()]);
    }
    
    public static string ToName(this Dictionary<byte[], object> infoDictionary)
    {
        return Encoding.UTF8.GetString((byte[])infoDictionary["name"u8.ToArray()]);
    }
    
    public static long ToLength(this Dictionary<byte[], object> infoDictionary)
    {
        return (long)infoDictionary["length"u8.ToArray()];
    }
    
    public static long ToPieceLength(this Dictionary<byte[], object> infoDictionary)
    {
        return (long)infoDictionary["piece length"u8.ToArray()];
    }
    
    public static List<string> ToPieceHashes(this byte[] piecesInBytes)
    {
        var pieces = new List<string>();
        for (var i = 0; i < piecesInBytes.Length; i += 20)
        {
            var piece = piecesInBytes[i..(i + 20)];
            pieces.Add(BitConverter.ToString(piece).Replace("-", "").ToLower());
        }

        return pieces;
    }

    private static string CalculateInfoHash(this byte[] bencodedBytes)
    {
        var infoHash = SHA1.HashData(bencodedBytes);

        var infoHashString = BitConverter.ToString(infoHash)
            .Replace("-", "").ToLower();

        return infoHashString;
    }
}