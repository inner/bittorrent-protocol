using codecrafters_bittorrent.Connection;
using codecrafters_bittorrent.Extensions;
using System.Collections.Concurrent;

namespace codecrafters_bittorrent.Commands;

public class Download : IBCommand
{
    public async Task Execute(string[] args)
    {
        var fileLocation = args[2];
        var torrentFilename = args[3];
        
        var torrent = new Torrent(await File.ReadAllBytesAsync(torrentFilename));
        var peers = await TrackerExtensions.DiscoverPeers(
            torrent.TrackerUrl,
            torrent.InfoHash,
            leftLength: torrent.Length);
        
        var pieceQueue = new ConcurrentQueue<int>(Enumerable.Range(0, torrent.PieceHashes.Count));
        var fileData = new byte[torrent.Length];
        var tasks = new List<Task>();

        foreach (var peer in peers)
        {
            tasks.Add(Task.Run(async () =>
            {
                using var peerConnection = new PeerConnection(torrent.InfoHash, peer);
                var (ns, _) = await peerConnection.Handshake();
                ns.Unchoke();

                while (pieceQueue.TryDequeue(out var pieceIndex))
                {
                    try
                    {
                        var piece = await ns.DownloadTorrentPiece(torrent, pieceIndex);
                        piece.CopyTo(fileData, pieceIndex * torrent.PieceLength);
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
        await File.WriteAllBytesAsync(fileLocation, fileData);
        Console.WriteLine($"Download completed: '{fileLocation}'.");
    }
}