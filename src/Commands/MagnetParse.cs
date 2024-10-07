using System.Net;

namespace codecrafters_bittorrent.Commands;

public class MagnetParse : IBCommand
{
    public Task Execute(string[] args)
    {
        var magnet = args[1];
        
        var trackerUrl = magnet.Split("&tr=")[1];
        trackerUrl = WebUtility.UrlDecode(trackerUrl);
        
        var infoHash = magnet.Split("&dn=")[0].Split(":")[3];
        
        Console.WriteLine($"Tracker URL: {trackerUrl}");
        Console.WriteLine($"Info Hash: {infoHash}");
        
        return Task.CompletedTask;
    }
}