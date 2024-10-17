using codecrafters_bittorrent.Commands;

var programArgs = args
    .Select(x => x.Replace("\"", string.Empty))
    .ToArray();

var command = programArgs[0] ?? throw new ArgumentNullException(programArgs[0]);

var commandDictionary = new Dictionary<string, ICommand>
{
    { "decode", new Decode() },
    { "info", new Info() },
    { "peers", new Peers() },
    { "handshake", new Handshake() },
    { "download_piece", new DownloadPiece() },
    { "download", new Download() },
    { "magnet_parse", new MagnetParse() },
    { "magnet_handshake", new MagnetHandshake() },
    { "magnet_info", new MagnetInfo() },
    { "magnet_download_piece", new MagnetDownloadPiece() },
    { "magnet_download", new MagnetDownload() }
};

if (commandDictionary.TryGetValue(command, out var commandToExecute))
{
    await commandToExecute.Execute(args);
}
else
{
    throw new ArgumentException($"Unknown command: {command}");
}