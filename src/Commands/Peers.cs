using codecrafters_bittorrent.Extensions;
using codecrafters_bittorrent.MetainfoFile;

namespace codecrafters_bittorrent.Commands;

public class Peers : IBCommand
{
    public async Task Execute(string[] args)
    {
        var torrentBytes = await File.ReadAllBytesAsync(args[1] ?? throw new ArgumentNullException(args[1]));
        var torrent = new Torrent(torrentBytes);

        var peers = await TrackerExtensions.DiscoverPeers(
            torrent.TrackerUrl,
            torrent.InfoHash,
            leftLength: torrent.Length);
        
        foreach (var peer in peers)
        {
            Console.WriteLine($"{peer.Ip}:{peer.Port}");
        }
    }
}