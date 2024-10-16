using codecrafters_bittorrent.Extensions;

namespace codecrafters_bittorrent;

public abstract class Metafile
{
    private byte[] PiecesInBytes => InfoDictionary.ToPiecesInBytes();
    public abstract Dictionary<byte[], object> InfoDictionary { get; set; }
    protected abstract byte[] InfoHash { get; }
    public string InfoHashHex => InfoHash.ToInfoHashHex();
    public string Name => InfoDictionary.ToName();
    public abstract string TrackerUrl { get; }
    public long Length => InfoDictionary.ToLength();
    public long PieceLength => InfoDictionary.ToPieceLength();
    public List<string> PieceHashes => PiecesInBytes.ToPieceHashes();
}