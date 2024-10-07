using System.Net;

namespace codecrafters_bittorrent.Commands;

public class MagnetParse : IBCommand
{
    public Task Execute(string[] args)
    {
        var magnet = args[1];
        // magnet:?xt=urn:btih:ad42ce8109f54c99613ce38f9b4d87e70f24a165&dn=magnet1.gif&tr=http%3A%2F%2Fbittorrent-test-tracker.codecrafters.io%2Fannounce
        
        // parse Tracker URL
        var trackerUrl = magnet.Split("&tr=")[1];
        trackerUrl = WebUtility.UrlDecode(trackerUrl);
        Console.WriteLine($"Tracker URL: {trackerUrl}");
        // parse Info Hash
        var infoHash = magnet.Split("&dn=")[0].Split(":")[3];
        Console.WriteLine($"Info Hash: {infoHash}");
        
        return Task.CompletedTask;
    }
}