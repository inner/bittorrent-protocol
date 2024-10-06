using System.Net.Sockets;

namespace codecrafters_bittorrent.Connection;

public class PeerConnection(Torrent torrent, Peer peer)
{
    private NetworkStream networkStream = null!;
    private readonly TcpClient tcpClient = new(peer.Ip, peer.Port);

    public async Task DownloadPiece(int pieceIndex)
    {
        var handshakeData = await Handshake();

        if (networkStream == null)
            throw new InvalidOperationException("Handshake must be performed before downloading a piece");

        Console.WriteLine($"Handshake data: {BitConverter.ToString(handshakeData).Replace("-", "").ToLower()}");

        await ReadMessage(PeerMessageType.Bitfield);
        await SendInterested();
        await ReadMessage(PeerMessageType.Unchoke);
    }

    private async Task SendInterested()
    {
        var interestedMessage = new byte[] { 0, 0, 0, 1, (byte)PeerMessageType.Interested };
        await networkStream.WriteAsync(interestedMessage);
    }

    public async Task<byte[]> Handshake(string? peerIpPort = null)
    {
        var peer = peerIpPort ?? (await torrent.DiscoverPeers()).First();
        var peerIp = peer.Split(':')[0];
        var peerPort = int.Parse(peer.Split(':')[1]);
        Console.WriteLine($"Connecting to peer {peerIp}:{peerPort}");

        networkStream = tcpClient.GetStream();
        var handshakeMessage = HandshakeMessage.Create(torrent);
        await networkStream.WriteAsync(handshakeMessage);

        var responseBuffer = new byte[handshakeMessage.Length];
        const int maxRetries = 10;
        var retryCount = 0;
        while (retryCount < maxRetries)
        {
            var lengthRead = await networkStream.ReadAsync(responseBuffer);
            if (lengthRead > 0)
                break;

            Console.WriteLine($"[{retryCount+1}] Trying to get handshake response...");
            await Task.Delay(1000);
            retryCount++;
        }
        
        if (retryCount == maxRetries)
            throw new InvalidOperationException("Failed to get handshake response");

        Console.WriteLine("Handshake completed");
        var responsePeerId = responseBuffer[48..handshakeMessage.Length];
        Console.WriteLine($"Peer ID: {BitConverter.ToString(responsePeerId).Replace("-", "").ToLower()}");
        return responseBuffer;
    }

    private async Task ReadMessage(PeerMessageType peerMessageType)
    {
        var buffer = new byte[5];

        while (true)
        {
            _ = await networkStream.ReadAsync(buffer);
            var messageId = buffer[4];
            if (messageId != (byte)peerMessageType)
            {
                continue;
            }

            Console.WriteLine($"Received message ID: {(PeerMessageType)messageId}");
            break;
        }
    }
}