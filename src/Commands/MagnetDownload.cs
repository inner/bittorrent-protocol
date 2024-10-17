using codecrafters_bittorrent.Extensions;
using codecrafters_bittorrent.Metadata;

namespace codecrafters_bittorrent.Commands;

public class MagnetDownload : IBCommand
{
    public async Task Execute(string[] args)
    {
        var fileLocation = args[2];
        var magnetUri = args[3];
        
        var magnet = new Magnet(magnetUri);
        var peers = await TrackerExtensions.DiscoverPeers(
            magnet.TrackerUrl,
            magnet.InfoHash,
            leftLength: 999);
        
        var fileData = await NetworkStreamExtensions.DownloadConcurrently(peers, magnet);
        await File.WriteAllBytesAsync(fileLocation, fileData);
        
        Console.WriteLine($"Download completed: '{fileLocation}'.");
    }
}