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

        if (!response.SupportsExtensions())
            throw new InvalidOperationException("Peer does not support extensions.");

        SupportsExtensions = true;

        var extensionMessage = ExtensionHandshakeMessage.Create();
        ns.WriteAsync(extensionMessage)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        var extensionHandshakeResponse = ns.ReadMessage(PeerMessageType.Extension);
        var payload = extensionHandshakeResponse[5..];
        var extensionMessageId = GetExtensionMessageId(payload);

        var metadataRequestMessage = MetadataRequestMessage.Create(extensionMessageId, 0);
        ns.WriteAsync(metadataRequestMessage)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        var metadataResponseMessage = ns.ReadMessage(PeerMessageType.Extension);
        var index = GetDictionaryEndPositionIndex(ref metadataResponseMessage);
        SetInfoDictionary(metadataResponseMessage[index..]);
    }

    private void SetInfoDictionary(byte[] metadata)
    {
        var index = 0;
        InfoDictionary = BencodeDecoder.DecodeDictionary(ref metadata, ref index);
    }

    private static int GetDictionaryEndPositionIndex(ref byte[] metadataResponseMessage)
    {
        var index = 0;
        BencodeDecoder.Decode(ref metadataResponseMessage, ref index);
        return index;
    }

    private static byte GetExtensionMessageId(byte[] payload)
    {
        byte extensionMessageId = 0;

        var index = 0;
        var extensionHandshakeResponsePayload = BencodeDecoder.DecodeDictionary(ref payload, ref index);
        if (extensionHandshakeResponsePayload.TryGetValue("ut_metadata"u8.ToArray(), out var extensionMessageIdObj) &&
            extensionMessageIdObj is long extensionMessageIdLong)
        {
            extensionMessageId = (byte)extensionMessageIdLong;
        }

        if (extensionMessageId == 0)
            throw new InvalidOperationException("Extension message ID not found.");

        return extensionMessageId;
    }

    private Peer GetPeer()
    {
        return TrackerExtensions.DiscoverPeers(TrackerUrl, InfoHash, leftLength: 999)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult()
            .First();
    }
}