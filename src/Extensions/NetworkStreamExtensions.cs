using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Security.Cryptography;
using codecrafters_bittorrent.Connection;
using codecrafters_bittorrent.Connection.Messages;
using codecrafters_bittorrent.Metainfo;

namespace codecrafters_bittorrent.Extensions;

public static class NetworkStreamExtensions
{
    private const int BlockSize = 16384;

    public static async Task<byte[]> DownloadTorrentPiece(this NetworkStream ns, Torrent torrent, int pieceIndex)
    {
        return await DownloadPiece(
            ns,
            torrent.Length,
            torrent.PieceLength,
            torrent.PieceHashes[pieceIndex],
            pieceIndex);
    }

    public static async Task<byte[]> DownloadPiece(this NetworkStream ns, long length, long pieceLength,
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
                await ns.WriteAsync(requestMessage);
                var data = ns.ReadMessage(PeerMessageType.Piece);
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

    public static async Task<byte[]> DownloadConcurrently(List<Peer> peers, Metainfo.Metainfo metainfo)
    {
        var pieceQueue = new ConcurrentQueue<int>(Enumerable.Range(0, metainfo.PieceHashes.Count));
        var fileData = new byte[metainfo.Length];
        var tasks = new List<Task>();

        foreach (var peer in peers)
        {
            tasks.Add(Task.Run(async () =>
            {
                using var peerConnection = new PeerConnection(metainfo.InfoHash, peer);
                var (ns, _) = await peerConnection.Handshake();
                ns.Unchoke(extensionEnabled: metainfo is Magnet);

                while (pieceQueue.TryDequeue(out var pieceIndex))
                {
                    try
                    {
                        var piece = await ns.DownloadPiece(
                            metainfo.Length,
                            metainfo.PieceLength,
                            metainfo.PieceHashes[pieceIndex],
                            pieceIndex);
                        
                        piece.CopyTo(fileData, pieceIndex * metainfo.PieceLength);
                        Console.WriteLine($"Downloaded piece {pieceIndex} from peer '{peer.Ip}:{peer.Port}'");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Failed to download piece {pieceIndex} from peer '{peer.Ip}:{peer.Port}': {ex.Message}");
                        pieceQueue.Enqueue(pieceIndex);
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);
        return fileData;
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

    public static void Unchoke(this NetworkStream networkStream, bool extensionEnabled = false)
    {
        networkStream.ReadMessage(PeerMessageType.Bitfield);
        networkStream.SendInterested();

        if (extensionEnabled)
            networkStream.ReadMessage(PeerMessageType.Extension);

        networkStream.ReadMessage(PeerMessageType.Unchoke);
    }

    public static bool SupportsExtensions(this byte[] handshakeResponseBuffer)
    {
        // skip 20 bytes of protocol string ("19:BitTorrent protocol")
        // take the reserved 8 bytes
        var reservedBytes = handshakeResponseBuffer
            .Skip(20)
            .Take(8)
            .ToArray();

        // 0x10 = 0b00010000
        return (reservedBytes[5] & 0x10) != 0;
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

    private static int ReadMessageLength(this NetworkStream networkStream)
    {
        var messageLength = new byte[4];
        networkStream.ReadExactly(messageLength, 0, 4);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(messageLength);

        return BitConverter.ToInt32(messageLength.ToArray(), 0);
    }

    private static void SendInterested(this NetworkStream networkStream)
    {
        var interestedMessage = new byte[] { 0, 0, 0, 1, (byte)PeerMessageType.Interested };
        networkStream.Write(interestedMessage);
    }
}