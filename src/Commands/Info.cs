namespace codecrafters_bittorrent.Commands;

public class Info : IBCommand
{
    public Task Execute(string[] args)
    {
        var torrentFileName = args[1] ?? throw new ArgumentNullException(args[1]);
        var torrent1 = new Torrent(File.ReadAllBytes(torrentFileName));

        Console.WriteLine($"Tracker URL: {torrent1.TrackerUrl}");
        Console.WriteLine($"Length: {torrent1.Length}");
        Console.WriteLine($"Info Hash: {torrent1.InfoHashHex}");
        Console.WriteLine($"Piece Length: {torrent1.PieceLength}");
        
        Console.WriteLine("Piece Hashes:");
        foreach (var pieceHash in torrent1.PieceHashes)
        {
            Console.WriteLine(pieceHash);
        }
        
        return Task.CompletedTask;
    }
}