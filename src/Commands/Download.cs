using codecrafters_bittorrent.Extensions;
using codecrafters_bittorrent.Metadata;

namespace codecrafters_bittorrent.Commands;

public class Download : ICommand
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

        var fileData = await NetworkStreamExtensions.DownloadConcurrently(peers, torrent);
        await File.WriteAllBytesAsync(fileLocation, fileData);
        
        Console.WriteLine($"Download completed: '{fileLocation}'.");
    }
}