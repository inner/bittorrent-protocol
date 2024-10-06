using System.Net.Sockets;
using System.Security.Cryptography;

namespace codecrafters_bittorrent.Connection;

public class PeerConnection(Torrent torrent, Peer peer)
{
    private readonly TcpClient tcpClient = new(peer.Ip, peer.Port);
    private NetworkStream networkStream = null!;
    private const int BlockSize = 16384;

    public async Task<byte[]> DownloadPiece(int pieceIndex)
    {
        try
        {
            await Handshake();
            ReadMessage(PeerMessageType.Bitfield);
            await SendInterested();
            ReadMessage(PeerMessageType.Unchoke);

            var pieceLength = Math.Min(torrent.Length - pieceIndex * torrent.PieceLength, torrent.PieceLength);

            List<byte> pieceData = [];
            Console.WriteLine($"Downloading piece index: {pieceIndex}.");
            Console.WriteLine($"Piece Length: {pieceLength}");
            for (var i = 0; i < (double)pieceLength / BlockSize; i++)
            {
                var blockOffset = i * BlockSize;
                var blockSize = Math.Min(BlockSize, (int)pieceLength - i * BlockSize);
                Console.WriteLine($"\t[{i}] Block Offset: {blockOffset}, Block Size: {blockSize}");
                var requestMessage = BlockRequestMessage.Create(pieceIndex, blockOffset, blockSize);
                await networkStream.WriteAsync(requestMessage);
                var data = ReadMessage(PeerMessageType.Piece);
                pieceData.AddRange(data[8..]);
            }

            if (!VerifyPieceIntegrity(pieceData.ToArray(), torrent.PieceHashes[pieceIndex]))
            {
                throw new InvalidOperationException("Piece integrity verification failed");
            }

            Console.WriteLine($"Downloaded piece index: {pieceIndex}.");

            return pieceData.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        finally
        {
            networkStream.Close();
            tcpClient.Close();
        }
    }

    private static bool VerifyPieceIntegrity(byte[] pieceBytes, string originalHash)
    {
        return Convert.ToHexString(SHA1.HashData(pieceBytes))
            .Equals(originalHash, StringComparison.CurrentCultureIgnoreCase);
    }

    public async Task<byte[]> Handshake()
    {
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
        var messageIdByte = ReadMessageId();
        if (messageIdByte != (byte)messageId)
        {
            throw new Exception($"Expected messageId: {messageId}, but received: {messageIdByte}");
        }

        var data = new byte[messageLength - 1];
        networkStream.ReadExactly(data, 0, data.Length);
        return data;
    }

    private int ReadMessageLength()
    {
        var messageLength = new byte[4];
        networkStream.ReadExactly(messageLength, 0, 4);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(messageLength);

        return BitConverter.ToInt32(messageLength.ToArray(), 0);
    }

    private byte ReadMessageId()
    {
        return (byte)networkStream.ReadByte();
    }

    private async Task SendInterested()
    {
        var interestedMessage = new byte[] { 0, 0, 0, 1, (byte)PeerMessageType.Interested };
        await networkStream.WriteAsync(interestedMessage);
    }
}