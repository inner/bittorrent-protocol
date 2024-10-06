using System.Text;

namespace codecrafters_bittorrent.Connection;

public static class HandshakeMessage
{
    public static byte[] Create(Torrent torrent, string? peerIdentification = null)
    {
        var protocolStringLengthBytes = (byte)19;
        var protocolStringBytes = "BitTorrent protocol"u8.ToArray();
        var reservedBytes = new byte[8];
        var infoHash = torrent.InfoHash;
        
        var peerId = peerIdentification != null
            ? Encoding.UTF8.GetBytes(peerIdentification)
            : Encoding.UTF8.GetBytes(GeneratePeerId(20));

        var handshakeBytes = new List<byte> { protocolStringLengthBytes };
        handshakeBytes.AddRange(protocolStringBytes);
        handshakeBytes.AddRange(reservedBytes);
        handshakeBytes.AddRange(infoHash);
        handshakeBytes.AddRange(peerId);
        
        return handshakeBytes.ToArray();
    }
    
    private static string GeneratePeerId(int length)
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}