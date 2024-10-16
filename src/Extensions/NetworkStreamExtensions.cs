using System.Net.Sockets;
using System.Security.Cryptography;
using codecrafters_bittorrent.Connection;
using codecrafters_bittorrent.Connection.Messages;

namespace codecrafters_bittorrent.Extensions;

public static class NetworkStreamExtensions
{
    private const int BlockSize = 16384;

    public static async Task<byte[]> DownloadTorrentPiece(this NetworkStream networkStream, Torrent torrent,
        int pieceIndex)
    {
        return await DownloadPiece(
            networkStream,
            torrent.Length,
            torrent.PieceLength,
            torrent.PieceHashes[pieceIndex],
            pieceIndex);
    }

    public static async Task<byte[]> DownloadPiece(this NetworkStream networkStream, long length, long pieceLength,
        string pieceHash, int pieceIndex)
    {
        try
        {
            pieceLength = Math.Min(length - pieceIndex * pieceLength, pieceLength);

            Console.WriteLine($"Downloading piece index: {pieceIndex}.");
            Console.WriteLine($"Piece Length: {pieceLength}");

            List<byte> pieceData = [];
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

            VerifyPieceIntegrity(pieceData.ToArray(), pieceHash, pieceIndex);
            Console.WriteLine($"Downloaded piece index: {pieceIndex}.");
            return pieceData.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    private static void VerifyPieceIntegrity(byte[] pieceBytes, string originalHash, int pieceIndex)
    {
        if (!Convert.ToHexString(SHA1.HashData(pieceBytes))
                .Equals(originalHash, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new InvalidOperationException($"Piece {pieceIndex} integrity verification failed");
        }

        Console.WriteLine($"Piece {pieceIndex} integrity verified");
    }

    public static byte[] ReadMessage(this NetworkStream networkStream, PeerMessageType messageIdType)
    {
        var messageLength = networkStream.ReadMessageLength();
        var messageId = (byte)networkStream.ReadByte();
        
        if (messageId != (byte)messageIdType)
            throw new Exception($"Expected messageIdType: {messageIdType}, but received: {messageId}");

        var message = new byte[messageLength - 1];
        networkStream.ReadExactly(message, 0, message.Length);
        return message;
    }

    private static int ReadMessageLength(this NetworkStream networkStream)
    {
        var messageLength = new byte[4];
        networkStream.ReadExactly(messageLength, 0, 4);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(messageLength);

        return BitConverter.ToInt32(messageLength.ToArray(), 0);
    }

    public static void SendInterested(this NetworkStream networkStream)
    {
        var interestedMessage = new byte[] { 0, 0, 0, 1, (byte)PeerMessageType.Interested };
        networkStream.Write(interestedMessage);
    }

    public static void Unchoke(this NetworkStream networkStream)
    {
        networkStream.ReadMessage(PeerMessageType.Bitfield);
        networkStream.SendInterested();
        networkStream.ReadMessage(PeerMessageType.Unchoke);
    }

    public static bool SupportsExtensions(this byte[] handshakeResponseBuffer)
    {
        var reservedBytes = handshakeResponseBuffer
            // skip protocol string ("19:BitTorrent protocol")
            .Skip(20)
            // take the next 8 bytes (reserved bytes)
            .Take(8)
            .ToArray();

        // 0x10 = 0b00010000
        return (reservedBytes[5] & 0x10) != 0;
    }
}