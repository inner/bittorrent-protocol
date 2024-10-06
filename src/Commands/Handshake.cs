using System.Text;
using codecrafters_bittorrent.Connection;

namespace codecrafters_bittorrent.Commands;

public class Handshake : IBCommand
{
    public async Task Execute(string[] args)
    {
        var torrent = new Torrent(await File.ReadAllBytesAsync(args[1]));
        
        var peerIpPort = args.Length > 2
            ? args[2]
            : (await torrent.DiscoverPeers()).First();
        
        var peerIp = peerIpPort.Split(':')[0];
        var peerPort = peerIpPort.Split(':')[1];
        
        var peerConnection = new PeerConnection(torrent, new Peer(peerIp, int.Parse(peerPort)));
        var response = await peerConnection.Handshake();
        
        var responseProtocolStringLength = response[0];
        var responseProtocolString = Encoding.UTF8.GetString(response, 1, 19);
        var responseReserved = response[20..28];
        var responseInfoHash = response[28..48];
        var responsePeerId = response[48..response.Length];
        
        Console.WriteLine($"Protocol string length: {responseProtocolStringLength}");
        Console.WriteLine($"Protocol string: {responseProtocolString}");
        Console.WriteLine($"Reserved: {BitConverter.ToString(responseReserved).Replace("-", "").ToLower()}");
        Console.WriteLine($"Info hash: {BitConverter.ToString(responseInfoHash).Replace("-", "").ToLower()}");
        Console.WriteLine($"Peer ID: {BitConverter.ToString(responsePeerId).Replace("-", "").ToLower()}");
    }
}