namespace codecrafters_bittorrent.Commands;

public interface ICommand
{
    Task Execute(string[] args);
}