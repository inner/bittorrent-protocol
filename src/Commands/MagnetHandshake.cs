using codecrafters_bittorrent.Connection;
using codecrafters_bittorrent.Extensions;

namespace codecrafters_bittorrent.Commands;

public class MagnetHandshake : IBCommand
{
    public async Task Execute(string[] args)
    {
        var magnet = args[1];
        var trackerUrl = magnet.GetTrackerUrl();
        var infoHash = magnet.GetInfoHash();

        var peer = (await TrackerExtensions.DiscoverPeers(trackerUrl, infoHash, 999)).First();
        using var peerConnection = new PeerConnection(infoHash, peer);
        var (networkStream, response) = await peerConnection.Handshake();
        networkStream.ReadMessage(PeerMessageType.Bitfield);
        
        if (!response.SupportsExtensions())
        {
            Console.WriteLine("Peer does not support extensions.");
            return;
        }
        
        var extensionMessage = ExtensionHandshakeMessage.Create();
        await networkStream.WriteAsync(extensionMessage);
    }
}