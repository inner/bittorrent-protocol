using System.Text;

namespace codecrafters_bittorrent.Connection;

public static class HandshakeMessage
{
    public static byte[] Create(byte[] infoHash, string? peerIdentification = null)
    {
        var protocolStringLengthBytes = (byte)19;
        var protocolStringBytes = "BitTorrent protocol"u8.ToArray();
        var reservedBytes = new byte[8];
        // Set the 6th byte to 0b00010000 to indicate that we support the extension protocol
        reservedBytes[5] = 0b00010000;
        
        var peerId = peerIdentification != null
            ? Encoding.UTF8.GetBytes(peerIdentification)
            : Encoding.UTF8.GetBytes(GeneratePeerId());

        var handshakeBytes = new List<byte> { protocolStringLengthBytes };
        handshakeBytes.AddRange(protocolStringBytes);
        handshakeBytes.AddRange(reservedBytes);
        handshakeBytes.AddRange(infoHash);
        handshakeBytes.AddRange(peerId);
        
        return handshakeBytes.ToArray();
    }
    
    private static string GeneratePeerId()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 20)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}