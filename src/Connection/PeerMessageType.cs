namespace codecrafters_bittorrent.Connection;

// https://www.bittorrent.org/beps/bep_0003.html#peer-messages
public enum PeerMessageType
{
    Choke = 0,
    Unchoke = 1,
    Interested = 2,
    NotInterested = 3,
    Have = 4,
    Bitfield = 5,
    Request = 6,
    Piece = 7,
    Cancel = 8,
    Port = 9,
    Extension = 20
}