using System.Net.Sockets;
using System.Security.Cryptography;

namespace codecrafters_bittorrent.Connection;

public static class PeerConnectionExtensions
{
    private const int BlockSize = 16384;
    
    public static async Task<byte[]> DownloadPiece(this NetworkStream networkStream, Torrent torrent, int pieceIndex)
    {
        try
        {
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
                var data = networkStream.ReadMessage(PeerMessageType.Piece);
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
    }
    
    private static bool VerifyPieceIntegrity(byte[] pieceBytes, string originalHash)
    {
        return Convert.ToHexString(SHA1.HashData(pieceBytes))
            .Equals(originalHash, StringComparison.CurrentCultureIgnoreCase);
    }


    public static byte[] ReadMessage(this NetworkStream networkStream, PeerMessageType messageId)
    {
        var messageLength = networkStream.ReadMessageLength();
        var messageIdByte = networkStream.ReadMessageId();
        if (messageIdByte != (byte)messageId)
        {
            throw new Exception($"Expected messageId: {messageId}, but received: {messageIdByte}");
        }

        var data = new byte[messageLength - 1];
        networkStream.ReadExactly(data, 0, data.Length);
        return data;
    }

    private static int ReadMessageLength(this NetworkStream networkStream)
    {
        var messageLength = new byte[4];
        networkStream.ReadExactly(messageLength, 0, 4);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(messageLength);

        return BitConverter.ToInt32(messageLength.ToArray(), 0);
    }

    private static byte ReadMessageId(this NetworkStream networkStream)
    {
        return (byte)networkStream.ReadByte();
    }

    public static async Task SendInterested(this NetworkStream networkStream)
    {
        var interestedMessage = new byte[] { 0, 0, 0, 1, (byte)PeerMessageType.Interested };
        await networkStream.WriteAsync(interestedMessage);
    }
}