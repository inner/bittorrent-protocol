using codecrafters_bittorrent.Connection;

namespace codecrafters_bittorrent.Commands;

public class Download : IBCommand
{
    public async Task Execute(string[] args)
    {
        var fileLocation = args[2];
        var torrentFilename = args[3];
        var torrent = new Torrent(await File.ReadAllBytesAsync(torrentFilename));
        
        var peerList = await torrent.DiscoverPeers();
        var random = new Random();
        var peer = peerList[random.Next(peerList.Count)];
        var peerIp = peer.Split(':')[0];
        var peerPort = peer.Split(':')[1];
        
        using var peerConnection = new PeerConnection(torrent, new Peer(peerIp, int.Parse(peerPort)));
        var networkStream = await peerConnection.Handshake();
        
        networkStream.ReadMessage(PeerMessageType.Bitfield);
        await networkStream.SendInterested();
        networkStream.ReadMessage(PeerMessageType.Unchoke);

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