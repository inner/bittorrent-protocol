using codecrafters_bittorrent.Connection;
using codecrafters_bittorrent.Extensions;
using codecrafters_bittorrent.Metainfo;

namespace codecrafters_bittorrent.Commands;

public class MagnetDownloadPiece : IBCommand
{
    public async Task Execute(string[] args)
    {
        var pieceLocation = args[2];
        var magnetUri = args[3];
        var pieceIndex = int.Parse(args[4]);

        var magnet = new Magnet(magnetUri);
        var peer = (await TrackerExtensions.DiscoverPeers(
                magnet.TrackerUrl,
                magnet.InfoHash,
                leftLength: 999))
            .First();

        var peerConnection = new PeerConnection(magnet.InfoHash, peer);
        var (ns, _) = await peerConnection.Handshake();
        ns.Unchoke(extensionEnabled: true);

        var pieceData = await ns.DownloadPiece(
            magnet.Length,
            magnet.PieceLength,
            magnet.PieceHashes[pieceIndex],
            pieceIndex);

        await File.WriteAllBytesAsync(pieceLocation, pieceData);
        Console.WriteLine($"Piece downloaded to {pieceLocation}");
    }
}