using codecrafters_bittorrent.Connection;
using codecrafters_bittorrent.Extensions;

namespace codecrafters_bittorrent.Commands;

public class DownloadPiece : IBCommand
{
    public async Task Execute(string[] args)
    {
        var pieceLocation = args[2];
        var torrentFilename = args[3];
        var pieceIndex = int.Parse(args[4]);

        var torrent = new Torrent(await File.ReadAllBytesAsync(torrentFilename));
        var peer = (await TrackerExtensions.DiscoverPeers(
                torrent.TrackerUrl,
                torrent.InfoHash,
                leftLength: torrent.Length))
            .First();

        using var peerConnection = new PeerConnection(torrent.InfoHash, peer);
        var (networkStream, _) = await peerConnection.Handshake();
        networkStream.Unchoke();
        
        var pieceData = await networkStream.DownloadTorrentPiece(torrent, pieceIndex);
        
        await File.WriteAllBytesAsync(pieceLocation, pieceData);
        Console.WriteLine($"Piece downloaded to {pieceLocation}");
    }
}