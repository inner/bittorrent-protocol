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
            : "00112233445566778899"u8.ToArray();

        var handshakeBytes = new List<byte> { protocolStringLengthBytes };
        handshakeBytes.AddRange(protocolStringBytes);
        handshakeBytes.AddRange(reservedBytes);
        handshakeBytes.AddRange(infoHash);
        handshakeBytes.AddRange(peerId);
        
        return handshakeBytes.ToArray();
    }
}