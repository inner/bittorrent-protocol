using codecrafters_bittorrent.Bencoding;

namespace codecrafters_bittorrent.Connection.Messages;

public static class MetadataRequestMessage
{
    public static byte[] Create(byte extensionMessageId, int pieceIndex)
    {
        var metadataRequest = new Dictionary<byte[], object>
        {
            { "msg_type"u8.ToArray(), 0 },
            { "piece"u8.ToArray(), pieceIndex }
        };
        
        List<byte> payload = [extensionMessageId];
        payload.AddRange(BencodeEncoder.Encode(metadataRequest));
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