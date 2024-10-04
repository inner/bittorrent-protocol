namespace codecrafters_bittorrent.Commands;

public interface IBCommand
{
    Task Execute(string[] args);
}