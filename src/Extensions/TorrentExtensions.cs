using System.Security.Cryptography;
using System.Text;
using codecrafters_bittorrent.Bencoding;

namespace codecrafters_bittorrent.Extensions;

public static class TorrentExtensions
{
    public static BencodingType ToBencodingType(this byte[] encodedValue, ref int index)
    {
        if (encodedValue == null || encodedValue.Length == 0)
            throw new ArgumentNullException(nameof(encodedValue));

        // increase if the first character is 0x04 (EoT)
        if (encodedValue[index] == 0x04)
            index++;

        if (index >= encodedValue.Length)
            throw new InvalidOperationException("No valid Bencode type found in the encoded value.");

        if (char.IsDigit((char)encodedValue[index]))
            return BencodingType.String;

        return encodedValue[index] switch
        {
            BencodingIndicators.IntegerIndicator => BencodingType.Integer,
            BencodingIndicators.ListIndicator => BencodingType.List,
            BencodingIndicators.DictionaryIndicator => BencodingType.Dictionary,
            _ => throw new InvalidOperationException(
                $"Invalid encoded value: [{index}]: {Encoding.UTF8.GetString(encodedValue)}")
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

    public static byte[] ToInfoHash(this Dictionary<byte[], object> infoDictionary)
    {
        var bencodedInfo = BencodeEncoder.EncodeDictionary(infoDictionary);
        return SHA1.HashData(bencodedInfo);
    }

    public static string ToInfoHashHex(this byte[] infoHash)
    {
        return BitConverter.ToString(infoHash).Replace("-", "").ToLower();
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
}