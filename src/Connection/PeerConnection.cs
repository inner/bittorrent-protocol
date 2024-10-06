using System.Net.Sockets;

namespace codecrafters_bittorrent.Connection;

public class PeerConnection(Torrent torrent, Peer peer)
{
    private readonly TcpClient tcpClient = new(peer.Ip, peer.Port);
    private readonly Torrent _torrent = torrent;
    private NetworkStream networkStream = null!;

    private const int BlockSize = 16384;

    public async Task<byte[]> DownloadPiece(int pieceIndex)
    {
        var handshakeData = await Handshake();

        if (networkStream == null)
            throw new InvalidOperationException("Handshake must be performed before downloading a piece");

        Console.WriteLine($"Handshake data: {BitConverter.ToString(handshakeData).Replace("-", "").ToLower()}");

        await ReadMessage(PeerMessageType.Bitfield);
        await SendInterested();
        await ReadMessage(PeerMessageType.Unchoke);

        List<byte> pieceData = [];
        for (var i = 0; i < (double)_torrent.PieceLength / BlockSize; i++)
        {
            var requestMessage = RequestBlockMessage.Create(pieceIndex, i * BlockSize,
                Math.Min(BlockSize, (int)_torrent.PieceLength - i * BlockSize));
            await networkStream.WriteAsync(requestMessage);

            pieceData.AddRange(await ReadMessage(PeerMessageType.Piece));
        }

        return pieceData.ToArray();
    }

    public async Task<byte[]> Handshake(string? peerIpPort = null)
    {
        var peer = peerIpPort ?? (await _torrent.DiscoverPeers()).First();
        var peerIp = peer.Split(':')[0];
        var peerPort = int.Parse(peer.Split(':')[1]);
        Console.WriteLine($"Connecting to peer {peerIp}:{peerPort}");

        networkStream = tcpClient.GetStream();
        var handshakeMessage = HandshakeMessage.Create(_torrent);
        await networkStream.WriteAsync(handshakeMessage);

        var responseBuffer = new byte[handshakeMessage.Length];
        const int maxRetries = 10;
        var retryCount = 0;
        while (retryCount < maxRetries)
        {
            var lengthRead = await networkStream.ReadAsync(responseBuffer);
            if (lengthRead > 0)
                break;

            Console.WriteLine($"[{retryCount + 1}] Trying to get handshake response...");
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

    private async Task<byte[]> ReadMessage(PeerMessageType peerMessageType)
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

        return buffer;
    }

    private async Task SendInterested()
    {
        var interestedMessage = new byte[] { 0, 0, 0, 1, (byte)PeerMessageType.Interested };
        await networkStream.WriteAsync(interestedMessage);
    }
}