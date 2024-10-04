using System.Net;
using System.Net.Sockets;
using System.Text;

namespace codecrafters_bittorrent.Commands;

public class Handshake : IBCommand
{
    public async Task Execute(string[] args)
    {
        var torrent = new Torrent(await File.ReadAllBytesAsync(args[1]));
        var peerToConnect = args[2];
        
        var protocolStringLengthBytes = (byte)19;
        var protocolStringBytes = "BitTorrent protocol"u8.ToArray();
        var reservedBytes = new byte[8];
        var infoHash = torrent.InfoHash;
        var peerId = "00112233445566778899"u8.ToArray();

        var handshakeBytes = new List<byte>
        {
            protocolStringLengthBytes
        };
        
        handshakeBytes.AddRange(protocolStringBytes);
        handshakeBytes.AddRange(reservedBytes);
        handshakeBytes.AddRange(infoHash);
        handshakeBytes.AddRange(peerId);
        
        var handshake = handshakeBytes.ToArray();
        
        // connect to the peer ip:port and send the handshake, receive the response
        var peerIp = peerToConnect.Split(':')[0];
        var peerPort = int.Parse(peerToConnect.Split(':')[1]);
        var peerEndPoint = new IPEndPoint(IPAddress.Parse(peerIp), peerPort);
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(peerEndPoint);
        await socket.SendAsync(handshake, SocketFlags.None);
        // read response
        var response = new byte[handshake.Length];
        await socket.ReceiveAsync(response, SocketFlags.None);
        // close the connection
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        // print peer id from response
        Console.WriteLine($"Length: {response.Length}");
    }
}