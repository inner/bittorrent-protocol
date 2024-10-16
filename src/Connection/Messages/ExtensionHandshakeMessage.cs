using codecrafters_bittorrent.Bencoding;

namespace codecrafters_bittorrent.Connection.Messages;

// https://www.bittorrent.org/beps/bep_0009.html#ut_metadata
// https://www.bittorrent.org/beps/bep_0010.html#handshake-message
public static class ExtensionHandshakeMessage
{
    public static byte[] Create()
    {
        var extensionMessage = new Dictionary<byte[], object>
        {
            ["m"u8.ToArray()] = new Dictionary<byte[], object>
            {
                ["ut_metadata"u8.ToArray()] = 4,
                ["ut_pex"u8.ToArray()] = 2
            },
            ["metadata_size"u8.ToArray()] = 0,
            ["v"u8.ToArray()] = "innerBittorrent v0.0.9"
        };

        List<byte> payload = [0];
        payload.AddRange(BencodeEncoder.Encode(extensionMessage));
        var payloadBytes = payload.ToArray();
        
        var lengthBytes = BitConverter.GetBytes(payloadBytes.Length + 1);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(lengthBytes);

        List<byte> message = [];
        message.AddRange(lengthBytes);
        message.Add((byte)PeerMessageType.Extension);
        message.AddRange(payloadBytes);
        return message.ToArray();
    }
}