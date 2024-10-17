using codecrafters_bittorrent.Metadata;

namespace codecrafters_bittorrent.Commands;

public class Info : ICommand
{
    public Task Execute(string[] args)
    {
        var torrentFileName = args[1] ?? throw new ArgumentNullException(args[1]);
        var torrent = new Torrent(File.ReadAllBytes(torrentFileName));

        Console.WriteLine($"Tracker URL: {torrent.TrackerUrl}");
        Console.WriteLine($"Length: {torrent.Length}");
        Console.WriteLine($"Info Hash: {torrent.InfoHashHex}");
        Console.WriteLine($"Piece Length: {torrent.PieceLength}");
        
        Console.WriteLine("Piece Hashes:");
        foreach (var pieceHash in torrent.PieceHashes)
        {
            Console.WriteLine(pieceHash);
        }
        
        return Task.CompletedTask;
    }
}