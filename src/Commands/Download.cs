using codecrafters_bittorrent.Connection;
using codecrafters_bittorrent.Extensions;

namespace codecrafters_bittorrent.Commands;

public class Download : IBCommand
{
    public async Task Execute(string[] args)
    {
        var fileLocation = args[2];
        var torrentFilename = args[3];
        var torrent = new Torrent(await File.ReadAllBytesAsync(torrentFilename));
        
        var peerList = await TrackerExtensions.DiscoverPeers(torrent.TrackerUrl, torrent.InfoHash, torrent.Length);
        var random = new Random();
        var peer = peerList[random.Next(peerList.Count)];
        
        using var peerConnection = new PeerConnection(torrent.InfoHash, peer);
        var networkStream = await peerConnection.Handshake();
        networkStream.Unchoke();

        Console.WriteLine($"Total pieces: {torrent.PieceHashes.Count}");
        var fileData = new byte[torrent.Length];
        for (var i = 0; i < torrent.PieceHashes.Count; i++)
        {
            var piece = await networkStream.DownloadPiece(torrent, i);
            piece.CopyTo(fileData, i * torrent.PieceLength);
        }
        
        await File.WriteAllBytesAsync(fileLocation, fileData);
        Console.WriteLine("Download completed");
    }
}