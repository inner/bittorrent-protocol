using codecrafters_bittorrent.Commands;

var programArgs = args
    .Select(x => x.Replace("\"", string.Empty))
    .ToArray();

var command = programArgs[0] ?? throw new ArgumentNullException(programArgs[0]);

switch (command)
{
    case "decode":
    {
        await new Decode().Execute(args);
        break;
    }
    case "info":
    {
        await new Info().Execute(args);
        break;
    }
    case "peers":
        await new Peers().Execute(args);
        break;
    case "handshake":
        await new Handshake().Execute(args);
        break;
    case "download_piece":
        await new DownloadPiece().Execute(args);
        break;
    case "download":
        await new Download().Execute(args);
        break;
    default:
        throw new InvalidOperationException($"Invalid command: {command}");
}