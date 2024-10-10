namespace codecrafters_bittorrent.Connection;

public static class BitfieldMessage
{
    public static byte[] Create()
    {
        List<byte> message = [];
        
        var lengthBytes = BitConverter.GetBytes(2);
        const byte messageType = (byte)PeerMessageType.Bitfield;
        
        // this is fake for now
        var bitfield = new byte[1 + 8];
        bitfield[0] = 0b11111111;
        bitfield[1] = 0b11111111;
        bitfield[2] = 0b11111111;
        bitfield[3] = 0b11111111;
        bitfield[4] = 0b11111111;
        bitfield[5] = 0b11111111;
        bitfield[6] = 0b11111111;
        bitfield[7] = 0b11111111;
        bitfield[8] = 0b11111111;
        
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(lengthBytes);
            Array.Reverse(bitfield);
        }
        
        message.AddRange(lengthBytes);
        message.Add(messageType);
        message.AddRange(bitfield);
        
        return message.ToArray();
    }
}