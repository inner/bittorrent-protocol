using codecrafters_bittorrent.Extensions;

namespace codecrafters_bittorrent.Metainfo;

public abstract class Metainfo
{
    protected Dictionary<byte[], object> InfoDictionary { get; init; } = null!;
    public abstract string TrackerUrl { get; }
    public string Name => InfoDictionary.ToName();
    public virtual byte[] InfoHash => InfoDictionary.ToInfoHash();
    public string InfoHashHex => InfoHash.ToInfoHashHex();
    public long Length => InfoDictionary.ToLength();
    private byte[] PiecesInBytes => InfoDictionary.ToPiecesInBytes();
    public long PieceLength => InfoDictionary.ToPieceLength();
    public List<string> PieceHashes => PiecesInBytes.ToPieceHashes();
}