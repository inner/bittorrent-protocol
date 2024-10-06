namespace codecrafters_bittorrent.Connection;

public class Peer(string ip, int port)
{
    public string Ip { get; } = ip;
    public int Port { get; } = port;
}