using System.Text;
using codecrafters_bittorrent.Extensions;

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
            : Encoding.UTF8.GetBytes(TrackerExtensions.GeneratePeerId());

        var handshakeBytes = new List<byte> { protocolStringLengthBytes };
        handshakeBytes.AddRange(protocolStringBytes);
        handshakeBytes.AddRange(reservedBytes);
        handshakeBytes.AddRange(infoHash);
        handshakeBytes.AddRange(peerId);
        
        return handshakeBytes.ToArray();
    }
}