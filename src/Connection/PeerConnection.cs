using System.Net.Sockets;
using System.Security.Cryptography;

namespace codecrafters_bittorrent.Connection;

public class PeerConnection
{
    private readonly TcpClient tcpClient;
    private readonly Torrent torrent;
    private NetworkStream networkStream = null!;
    private const int BlockSize = 16384;

    public PeerConnection(Torrent torrent, Peer peer)
    {
        this.torrent = torrent;
        tcpClient = new TcpClient(peer.Ip, peer.Port);
    }

    public async Task<byte[]> DownloadPiece(int pieceIndex)
    {
        var handshakeData = await Handshake();

        if (networkStream == null)
            throw new InvalidOperationException("Handshake must be performed before downloading a piece");

        Console.WriteLine($"Handshake data: {BitConverter.ToString(handshakeData).Replace("-", "").ToLower()}");

        ReadMessage(PeerMessageType.Bitfield);
        await SendInterested();
        ReadMessage(PeerMessageType.Unchoke);

        List<byte> pieceData = [];
        Console.WriteLine($"File size: {torrent.Length}");
        Console.WriteLine($"Piece size: {torrent.PieceLength}");
        for (var i = 0; i < (double)torrent.PieceLength / BlockSize; i++)
        {
            var blockOffset = i * BlockSize;
            var blockSize = Math.Min(BlockSize, (int)torrent.PieceLength - i * BlockSize);
            Console.WriteLine($"Piece Index: {pieceIndex}, Block Offset: {blockOffset}, Block Size: {blockSize}");
            var requestMessage = RequestBlockMessage.Create(pieceIndex, blockOffset, blockSize);
            await networkStream.WriteAsync(requestMessage);

            var data = ReadMessage(PeerMessageType.Piece);
            Console.WriteLine($"Data length: {data.Length}");
            pieceData.AddRange(data[8..]);
        }

        if (!VerifyPieceIntegrity(pieceData.ToArray(), torrent.PieceHashes[pieceIndex]))
        {
            throw new InvalidOperationException("Piece integrity verification failed");
        }

        return pieceData.ToArray();
    }

    private static bool VerifyPieceIntegrity(byte[] pieceBytes, string originalHash)
    {
        return Convert.ToHexString(SHA1.HashData(pieceBytes))
            .Equals(originalHash, StringComparison.CurrentCultureIgnoreCase);
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

    private byte[] ReadMessage(PeerMessageType messageId)
    {
        var messageLength = ReadMessageLength();
        var messageIdByte = networkStream.ReadByte();
        if (messageIdByte != (byte)messageId)
        {
            throw new Exception(
                $"Could not read messageId: {messageId}. Instead received {messageIdByte}");
        }

        var data = new byte[messageLength - 1];
        networkStream.ReadExactly(data, 0, data.Length);
        return data;
    }

    private int ReadMessageLength()
    {
        var messageLength = new byte[4];
        networkStream.ReadExactly(messageLength, 0, messageLength.Length);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(messageLength);

        return BitConverter.ToInt32(messageLength.ToArray(), 0);
    }

    private async Task SendInterested()
    {
        var interestedMessage = new byte[] { 0, 0, 0, 1, (byte)PeerMessageType.Interested };
        await networkStream.WriteAsync(interestedMessage);
    }
}