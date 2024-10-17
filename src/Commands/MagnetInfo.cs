using codecrafters_bittorrent.Bencoding;
using codecrafters_bittorrent.Connection;
using codecrafters_bittorrent.Connection.Messages;
using codecrafters_bittorrent.Extensions;

namespace codecrafters_bittorrent.Commands;

public class MagnetInfo : ICommand
{
    public async Task Execute(string[] args)
    {
        var magnet = args[1];
        var trackerUrl = magnet.GetTrackerUrl();
        var infoHash = magnet.GetInfoHash();

        var peer = (await TrackerExtensions.DiscoverPeers(
                trackerUrl,
                infoHash,
                leftLength: 999))
            .First();
        
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

        Console.WriteLine($"Tracker URL: {trackerUrl}");
        Console.WriteLine($"Length: {length}");
        Console.WriteLine($"Info Hash: {infoHash.ToInfoHashHex()}");
        Console.WriteLine($"Piece Length: {pieceLength}");
        Console.WriteLine("Piece Hashes:");
        foreach (var pieceHash in pieceHashes)
        {
            Console.WriteLine(pieceHash);
        }
    }
}