using System.Net.Sockets;

namespace codecrafters_bittorrent.Connection;

public class PeerConnection(byte[] infoHash, Peer peer) : IDisposable
{
    private readonly TcpClient tcpClient = new(peer.Ip, peer.Port);
    private NetworkStream? networkStream;

    public async Task<NetworkStream> Handshake()
    {
        networkStream = tcpClient.GetStream();
        var handshakeMessage = HandshakeMessage.Create(infoHash);
        await networkStream.WriteAsync(handshakeMessage);

        var responseBuffer = new byte[handshakeMessage.Length];
        const int maxRetries = 10;
        var retryCount = 0;
        while (retryCount < maxRetries)
        {
            var lengthRead = await networkStream.ReadAsync(responseBuffer);
            if (lengthRead > 0)
                break;

            Console.WriteLine($"[{retryCount + 1}] Trying to get handshake response...");
            await Task.Delay(1000);
            retryCount++;
        }

        if (retryCount == maxRetries)
            throw new InvalidOperationException("Failed to get handshake response");

        Console.WriteLine("Handshake completed");
        var responsePeerId = responseBuffer[48..handshakeMessage.Length];
        Console.WriteLine($"Peer ID: {BitConverter.ToString(responsePeerId).Replace("-", "").ToLower()}");
        
        return networkStream;
    }

    public void Dispose()
    {
        tcpClient.Dispose();
        networkStream?.Dispose();
    }
}