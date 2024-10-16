using codecrafters_bittorrent.Connection;
using codecrafters_bittorrent.Extensions;

namespace codecrafters_bittorrent.Commands;

public class MagnetDownloadPiece : IBCommand
{
    public async Task Execute(string[] args)
    {
        var pieceLocation = args[2];
        var magnetUri = args[3];
        var pieceIndex = int.Parse(args[4]);

        var magnet = new Magnet(magnetUri);

        var peer = (await TrackerExtensions.DiscoverPeers(magnet.TrackerUrl, magnet.InfoHash, 999)).First();
        // using var peerConnection = new PeerConnection(magnet.InfoHash, peer);
        // var (networkStream, response) = await peerConnection.Handshake();
        // networkStream.ReadMessage(PeerMessageType.Bitfield);
        //
        // if (!response.SupportsExtensions())
        // {
        //     Console.WriteLine("Peer does not support extensions.");
        //     return;
        // }
        //
        // var extensionMessage = ExtensionHandshakeMessage.Create();
        // await networkStream.WriteAsync(extensionMessage);
        //
        // var extensionHandshakeResponse = networkStream.ReadMessage(PeerMessageType.Extension);
        // var payload = extensionHandshakeResponse[5..];
        //
        // byte? extensionMessageId = null!;
        // var index = 0;
        // var extensionHandshakeResponsePayload = BencodeDecoder.DecodeDictionary(ref payload, ref index);
        // if (extensionHandshakeResponsePayload.TryGetValue("ut_metadata"u8.ToArray(), out var extensionMessageIdObj) &&
        //     extensionMessageIdObj is long extensionMessageIdLong)
        // {
        //     extensionMessageId = (byte)extensionMessageIdLong;
        // }
        //
        // var metadataRequestMessage = MetadataRequestMessage.Create(extensionMessageId.Value, 0);
        // await networkStream.WriteAsync(metadataRequestMessage);
        // var metadataResponseMessage = networkStream.ReadMessage(PeerMessageType.Extension);
        //
        // // skip the dictionary part of the message
        // index = 0;
        // BencodeDecoder.Decode(ref metadataResponseMessage, ref index);
        //
        // // get the metadata part of the message
        // var metadata = metadataResponseMessage[index..];
        // index = 0;
        // var infoDictionary = BencodeDecoder.DecodeDictionary(ref metadata, ref index);
        // var length = infoDictionary.ToLength();
        // var pieceLength = infoDictionary.ToPieceLength();
        // var piecesInBytes = (byte[])infoDictionary["pieces"u8.ToArray()];
        // var pieceHashes = piecesInBytes.ToPieceHashes();

        var peerDownloadConnection = new PeerConnection(magnet.InfoHash, peer);
        var (downloadNetworkStream, _) = await peerDownloadConnection.Handshake();
        downloadNetworkStream.ReadMessage(PeerMessageType.Bitfield);
        downloadNetworkStream.SendInterested();
        downloadNetworkStream.ReadMessage(PeerMessageType.Extension);
        downloadNetworkStream.ReadMessage(PeerMessageType.Unchoke);
        
        var pieceData = await downloadNetworkStream.DownloadPiece(
            magnet.Length,
            magnet.PieceLength,
            magnet.PieceHashes[pieceIndex],
            pieceIndex);
        
        await File.WriteAllBytesAsync(pieceLocation, pieceData);
        Console.WriteLine($"Piece downloaded to {pieceLocation}");
    }
}