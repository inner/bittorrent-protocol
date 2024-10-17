using codecrafters_bittorrent.Extensions;

namespace codecrafters_bittorrent;

public class Torrent : Metainfo
{
    private readonly Dictionary<byte[], object> TorrentDictionary;

    public Torrent(byte[] torrentBytes)
    {
        TorrentDictionary = torrentBytes.ToDictionary();
        InfoDictionary = TorrentDictionary.ToInfoDictionary();
    }

    public override string TrackerUrl => TorrentDictionary.ToTrackerUrl();
    public string InfoHashHex => InfoHash.ToInfoHashHex();
    public string Name => InfoDictionary.ToName();
}