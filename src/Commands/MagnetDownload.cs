using System.Collections.Concurrent;
using codecrafters_bittorrent.Connection;
using codecrafters_bittorrent.Extensions;

namespace codecrafters_bittorrent.Commands;

public class MagnetDownload : IBCommand
{
    public async Task Execute(string[] args)
    {
        var fileLocation = args[2];
        var magnetUri = args[3];

        var magnet = new Magnet(magnetUri);

        var pieceQueue = new ConcurrentQueue<int>(Enumerable.Range(0, magnet.PieceHashes.Count));
        var fileData = new byte[magnet.Length];
        var tasks = new List<Task>();

        var peers = await TrackerExtensions.DiscoverPeers(
            magnet.TrackerUrl, magnet.InfoHash, leftLength: 999);

        foreach (var peer in peers)
        {
            tasks.Add(Task.Run(async () =>
            {
                var peerConnection = new PeerConnection(magnet.InfoHash, peer);
                var (ns, _) = await peerConnection.Handshake();
                ns.Unchoke(extensionEnabled: true);

                while (pieceQueue.TryDequeue(out var pieceIndex))
                {
                    try
                    {
                        var piece = await ns.DownloadPiece(
                            magnet.Length,
                            magnet.PieceLength,
                            magnet.PieceHashes[pieceIndex],
                            pieceIndex);

                        piece.CopyTo(fileData, pieceIndex * magnet.PieceLength);
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