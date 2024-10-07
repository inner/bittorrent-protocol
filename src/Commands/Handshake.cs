using codecrafters_bittorrent.Connection;

namespace codecrafters_bittorrent.Commands;

public class Handshake : IBCommand
{
    public async Task Execute(string[] args)
    {
        var torrent = new Torrent(await File.ReadAllBytesAsync(args[1]));
        
        var peerIpPort = args.Length > 2
            ? args[2]
            : (await torrent.DiscoverPeers()).First();
        
        var peerIp = peerIpPort.Split(':')[0];
        var peerPort = peerIpPort.Split(':')[1];
        
        var peerConnection = new PeerConnection(torrent, new Peer(peerIp, int.Parse(peerPort)));
        _ = await peerConnection.Handshake();
    }
}