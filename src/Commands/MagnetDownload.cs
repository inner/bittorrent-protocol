using System.Collections.Concurrent;
using codecrafters_bittorrent.Bencoding;
using codecrafters_bittorrent.Connection;
using codecrafters_bittorrent.Connection.Messages;
using codecrafters_bittorrent.Extensions;

namespace codecrafters_bittorrent.Commands;

public class MagnetDownload : IBCommand
{
    public async Task Execute(string[] args)
    {
        var fileLocation = args[2];
        var magnet = args[3];

        var trackerUrl = magnet.GetTrackerUrl();
        var infoHash = magnet.GetInfoHash();

        var peerList = await TrackerExtensions.DiscoverPeers(trackerUrl, infoHash, 999);
        var peer = peerList.First();
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

        var extensionHandshakeResponse = networkStream.ReadMessage(PeerMessageType.Extension);
        var payload = extensionHandshakeResponse[5..];

        byte? extensionMessageId = null!;
        var index = 0;
        var extensionHandshakeResponsePayload = BencodeDecoder.DecodeDictionary(ref payload, ref index);
        if (extensionHandshakeResponsePayload.TryGetValue("ut_metadata"u8.ToArray(), out var extensionMessageIdObj) &&
            extensionMessageIdObj is long extensionMessageIdLong)
        {
            extensionMessageId = (byte)extensionMessageIdLong;
        }

        var metadataRequestMessage = MetadataRequestMessage.Create(extensionMessageId.Value, 0);
        await networkStream.WriteAsync(metadataRequestMessage);

        var metadataResponseMessage = networkStream.ReadMessage(PeerMessageType.Extension);

        // skip the dictionary part of the message
        index = 0;
        BencodeDecoder.Decode(ref metadataResponseMessage, ref index);

        // get the metadata part of the message
        var metadata = metadataResponseMessage[index..];
        index = 0;
        var infoDictionary = BencodeDecoder.DecodeDictionary(ref metadata, ref index);
        var length = infoDictionary.ToLength();
        var pieceLength = infoDictionary.ToPieceLength();
        var piecesInBytes = (byte[])infoDictionary["pieces"u8.ToArray()];
        var pieceHashes = piecesInBytes.ToPieceHashes();

        var pieceQueue = new ConcurrentQueue<int>(Enumerable.Range(0, pieceHashes.Count));
        var fileData = new byte[length];
        var tasks = new List<Task>();

        foreach (var downloadPeer in peerList)
        {
            tasks.Add(Task.Run(async () =>
            {
                var peerDownloadConnection = new PeerConnection(infoHash, peer);
                var (downloadNetworkStream, _) = await peerDownloadConnection.Handshake();
                downloadNetworkStream.ReadMessage(PeerMessageType.Bitfield);
                downloadNetworkStream.SendInterested();
                downloadNetworkStream.ReadMessage(PeerMessageType.Extension);
                downloadNetworkStream.ReadMessage(PeerMessageType.Unchoke);

                while (pieceQueue.TryDequeue(out var pieceIndex))
                {
                    try
                    {
                        var piece = await downloadNetworkStream.DownloadPiece(
                            length,
                            pieceLength,
                            pieceHashes[pieceIndex],
                            pieceIndex);

                        piece.CopyTo(fileData, pieceIndex * pieceLength);
                        Console.WriteLine(
                            $"Downloaded piece {pieceIndex} from peer '{downloadPeer.Ip}:{downloadPeer.Port}'");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Failed to download piece {pieceIndex} from peer '{downloadPeer.Ip}:{downloadPeer.Port}': {ex.Message}");
                        pieceQueue.Enqueue(pieceIndex);
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);
        await File.WriteAllBytesAsync(fileLocation, fileData);
        Console.WriteLine($"Download completed: '{fileLocation}'.");
    }
}