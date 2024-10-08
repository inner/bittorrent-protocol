using codecrafters_bittorrent.Extensions;

namespace codecrafters_bittorrent;

public class Torrent(byte[] torrentBytes)
{
    private Dictionary<byte[], object> TorrentDictionary => torrentBytes.ToDictionary();
    private Dictionary<byte[], object> InfoDictionary => TorrentDictionary.ToInfoDictionary();
    private byte[] PiecesInBytes => InfoDictionary.ToPiecesInBytes();
    public byte[] InfoHash => InfoDictionary.ToInfoHash();
    public string InfoHashHex => InfoHash.ToInfoHashHex();
    public string TrackerUrl => TorrentDictionary.ToTrackerUrl();
    public string Name => InfoDictionary.ToName();
    public long Length => InfoDictionary.ToLength();
    public long PieceLength => InfoDictionary.ToPieceLength();
    public List<string> PieceHashes => PiecesInBytes.ToPieceHashes();
}