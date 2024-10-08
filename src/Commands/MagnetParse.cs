using codecrafters_bittorrent.Extensions;

namespace codecrafters_bittorrent.Commands;

public class MagnetParse : IBCommand
{
    public Task Execute(string[] args)
    {
        var magnet = args[1];

        var trackerUrl = magnet.GetTrackerUrl();
        var infoHash = magnet.GetInfoHash();
        
        Console.WriteLine($"Tracker URL: {trackerUrl}");
        Console.WriteLine($"Info hash: {infoHash.ToInfoHashHex()}");
        
        return Task.CompletedTask;
    }
}