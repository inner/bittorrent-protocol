namespace codecrafters_bittorrent.Connection;

public static class BlockRequestMessage
{
    public static byte[] Create(int pieceIndex, int blockOffset, int blockSize)
    {
        List<byte> message = [];
        
        message.AddRange(BitConverter.GetBytes(13));
        message.Add((byte)PeerMessageType.Request);
        var pieceIndexArray = BitConverter.GetBytes(pieceIndex);
        var blockOffsetArray = BitConverter.GetBytes(blockOffset);
        var blockSizeArray = BitConverter.GetBytes(blockSize);
        
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(pieceIndexArray);
            Array.Reverse(blockOffsetArray);
            Array.Reverse(blockSizeArray);
        }
        
        message.AddRange(pieceIndexArray);
        message.AddRange(blockOffsetArray);
        message.AddRange(blockSizeArray);
        
        return message.ToArray();
    }
}