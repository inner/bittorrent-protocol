using codecrafters_bittorrent.Connection;
using codecrafters_bittorrent.Extensions;
using codecrafters_bittorrent.Metadata;

namespace codecrafters_bittorrent.Commands;

public class Handshake : ICommand
{
    public async Task Execute(string[] args)
    {
        var torrent = new Torrent(await File.ReadAllBytesAsync(args[1]));

        Peer peer;

        if (args.Length > 2)
        {
            var peerIpPort = args[2];
            var peerIp = peerIpPort.Split(':')[0];
            var peerPort = peerIpPort.Split(':')[1];
            peer = new Peer(peerIp, int.Parse(peerPort));
        }
        else
        {
            var peers = await TrackerExtensions.DiscoverPeers(
                torrent.TrackerUrl,
                torrent.InfoHash,
                leftLength: torrent.Length);
            peer = peers.First();
        }

        var peerConnection = new PeerConnection(torrent.InfoHash, peer);
        _ = await peerConnection.Handshake();
    }
}