using System.Net.Sockets;

namespace codecrafters_bittorrent.Connection;

public class PeerConnection(byte[] infoHash, Peer peer) : IDisposable
{
    private readonly TcpClient tcpClient = new(peer.Ip, peer.Port);
    private NetworkStream? networkStream;

    public async Task<(NetworkStream, byte[])> Handshake()
    {
        networkStream = tcpClient.GetStream();
        var handshakeMessage = HandshakeMessage.Create(infoHash);
        await networkStream.WriteAsync(handshakeMessage);

        var responseBuffer = new byte[handshakeMessage.Length];
        const int maxRetries = 1;
        var retryCount = 0;
        while (retryCount < maxRetries)
        {
            var lengthRead = await networkStream.ReadAsync(responseBuffer);
            if (lengthRead > 0)
                break;

            Console.WriteLine($"[{retryCount + 1}] Trying to get handshake response from peer '{peer.Ip}:{peer.Port}'.");
            await Task.Delay(1000);
            retryCount++;
        }

        if (retryCount == maxRetries)
            throw new InvalidOperationException($"Failed to get handshake response from peer '{peer.Ip}:{peer.Port}'.");

        Console.WriteLine($"Handshake with peer '{peer.Ip}:{peer.Port}' completed.");
        var responsePeerId = responseBuffer[48..handshakeMessage.Length];
        Console.WriteLine($"Peer ID: {BitConverter.ToString(responsePeerId).Replace("-", "").ToLower()}");
        
        return (networkStream, responseBuffer);
    }

    public void Dispose()
    {
        tcpClient.Dispose();
        networkStream?.Dispose();
    }
}