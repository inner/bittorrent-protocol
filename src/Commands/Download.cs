using codecrafters_bittorrent.Connection;

namespace codecrafters_bittorrent.Commands;

public class Download : IBCommand
{
    public async Task Execute(string[] args)
    {
        var fileLocation = args[2];
        var torrentFilename = args[3];
        var torrent = new Torrent(await File.ReadAllBytesAsync(torrentFilename));
        
        var peerIpPort = (await torrent.DiscoverPeers()).Skip(2).First();
        var peerIp = peerIpPort.Split(':')[0];
        var peerPort = peerIpPort.Split(':')[1];

        var fileData = new byte[torrent.Length];
        for (var i = 0; i < torrent.PieceHashes.Count; i++)
        {
            var peerConnection = new PeerConnection(torrent, new Peer(peerIp, int.Parse(peerPort)));
            var pieceData = await peerConnection.DownloadFile(i);
            pieceData.CopyTo(fileData, i * torrent.PieceLength);
        }
        
        await File.WriteAllBytesAsync(fileLocation, fileData);
        Console.WriteLine("Download completed");
    }
}