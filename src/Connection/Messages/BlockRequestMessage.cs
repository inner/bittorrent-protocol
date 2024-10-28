namespace codecrafters_bittorrent.Connection.Messages;

public static class BlockRequestMessage
{
    public static byte[] Create(int pieceIndex, int blockOffset, int blockSize)
    {
        List<byte> message = [];
        
        // this is fixed size length of the message
        // 4 bytes for length (excluding the length itself),
        // 1 byte for message type,
        // 4 bytes for piece index,
        // 4 bytes for block offset,
        // 4 bytes for block size
        var lengthBytes = BitConverter.GetBytes(13);
        var pieceIndexArray = BitConverter.GetBytes(pieceIndex);
        var blockOffsetArray = BitConverter.GetBytes(blockOffset);
        var blockSizeArray = BitConverter.GetBytes(blockSize);
        
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(lengthBytes);
            Array.Reverse(pieceIndexArray);
            Array.Reverse(blockOffsetArray);
            Array.Reverse(blockSizeArray);
        }
        
        message.AddRange(lengthBytes);
        message.Add((byte)PeerMessageType.Request);
        message.AddRange(pieceIndexArray);
        message.AddRange(blockOffsetArray);
        message.AddRange(blockSizeArray);
        
        return message.ToArray();
    }
}