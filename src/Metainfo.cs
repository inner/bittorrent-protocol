using codecrafters_bittorrent.Extensions;

namespace codecrafters_bittorrent;

public abstract class Metainfo
{
    protected Dictionary<byte[], object> InfoDictionary { get; init; } = null!;
    public abstract string TrackerUrl { get; }
    private byte[] PiecesInBytes => InfoDictionary.ToPiecesInBytes();
    public virtual byte[] InfoHash => InfoDictionary.ToInfoHash();
    public long Length => InfoDictionary.ToLength();
    public long PieceLength => InfoDictionary.ToPieceLength();
    public List<string> PieceHashes => PiecesInBytes.ToPieceHashes();
}