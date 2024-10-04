namespace codecrafters_bittorrent.Commands;

public class Handshake : IBCommand
{
    public async Task Execute(string[] args)
    {
        var torrent = new Torrent(await File.ReadAllBytesAsync(args[1]));
        foreach (var peer in await torrent.GetPeers())
        {
            Console.WriteLine(peer);
        }
    }
}