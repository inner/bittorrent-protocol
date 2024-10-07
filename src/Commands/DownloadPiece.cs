using codecrafters_bittorrent.Connection;

namespace codecrafters_bittorrent.Commands;

public class DownloadPiece : IBCommand
{
    public async Task Execute(string[] args)
    {
        var outputFlag = args[1];
        var pieceLocation = args[2];
        var torrentFilename = args[3];
        var pieceIndex = int.Parse(args[4]);

        var torrent = new Torrent(await File.ReadAllBytesAsync(torrentFilename));
        
        var peerIpPort = (await torrent.DiscoverPeers()).First();
        var peerIp = peerIpPort.Split(':')[0];
        var peerPort = peerIpPort.Split(':')[1];
        
        var peerConnection = new PeerConnection(torrent, new Peer(peerIp, int.Parse(peerPort)));
        var networkStream = await peerConnection.Handshake();
        networkStream.ReadMessage(PeerMessageType.Bitfield);
        await networkStream.SendInterested();
        networkStream.ReadMessage(PeerMessageType.Unchoke);
        
        var pieceData = await networkStream.DownloadPiece(torrent, pieceIndex);
        
        await File.WriteAllBytesAsync(pieceLocation, pieceData);
        Console.WriteLine($"Piece downloaded to {pieceLocation}");
    }
}