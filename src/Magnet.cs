using codecrafters_bittorrent.Bencoding;
using codecrafters_bittorrent.Connection;
using codecrafters_bittorrent.Connection.Messages;
using codecrafters_bittorrent.Extensions;

namespace codecrafters_bittorrent;

public class Magnet
{
    private readonly string magnetUri;
    private Dictionary<byte[], object> InfoDictionary { get; set; } = null!;
    private byte[] PiecesInBytes => InfoDictionary.ToPiecesInBytes();

    public Magnet(string magnetUri)
    {
        this.magnetUri = magnetUri;
        Initialize();
    }

    public string TrackerUrl => magnetUri.GetTrackerUrl();
    public byte[] InfoHash => magnetUri.GetInfoHash();
    public bool SupportsExtensions { get; private set; }
    public long Length => InfoDictionary.ToLength();
    public long PieceLength => InfoDictionary.ToPieceLength();
    public List<string> PieceHashes => PiecesInBytes.ToPieceHashes();

    private void Initialize()
    {
        var peer = GetPeer();

        using var peerConnection = new PeerConnection(InfoHash, peer);
        var (ns, response) = peerConnection.Handshake()
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
        ns.ReadMessage(PeerMessageType.Bitfield);

        if (response.SupportsExtensions())
            SupportsExtensions = true;

        var extensionMessage = ExtensionHandshakeMessage.Create();
        ns.WriteAsync(extensionMessage)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        var extensionHandshakeResponse = ns.ReadMessage(PeerMessageType.Extension);
        var payload = extensionHandshakeResponse[5..];

        byte? extensionMessageId = null!;
        var index = 0;
        var extensionHandshakeResponsePayload = BencodeDecoder.DecodeDictionary(ref payload, ref index);
        if (extensionHandshakeResponsePayload.TryGetValue("ut_metadata"u8.ToArray(), out var extensionMessageIdObj) &&
            extensionMessageIdObj is long extensionMessageIdLong)
        {
            extensionMessageId = (byte)extensionMessageIdLong;
        }

        var metadataRequestMessage = MetadataRequestMessage.Create(extensionMessageId.Value, 0);
        ns.WriteAsync(metadataRequestMessage)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        var metadataResponseMessage = ns.ReadMessage(PeerMessageType.Extension);

        // skip the dictionary part of the message
        index = 0;
        BencodeDecoder.Decode(ref metadataResponseMessage, ref index);

        // get the metadata part of the message
        var metadata = metadataResponseMessage[index..];
        index = 0;

        InfoDictionary = BencodeDecoder.DecodeDictionary(ref metadata, ref index);
    }

    private Peer GetPeer()
    {
        return TrackerExtensions.DiscoverPeers(TrackerUrl, InfoHash, 999)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult()
            .First();
    }
}