using codecrafters_bittorrent.Bencoding;
using codecrafters_bittorrent.Connection;
using codecrafters_bittorrent.Connection.Messages;
using codecrafters_bittorrent.Extensions;

namespace codecrafters_bittorrent.Metainfo;

public class Magnet : Metainfo
{
    private readonly string magnetUri;

    public Magnet(string magnetUri)
    {
        this.magnetUri = magnetUri;
        InfoDictionary = GetInfoDictionary();
    }

    public override string TrackerUrl => magnetUri.GetTrackerUrl();
    public override byte[] InfoHash => magnetUri.GetInfoHash();
    public bool SupportsExtensions { get; private set; }

    private Dictionary<byte[], object> GetInfoDictionary()
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
        var metadata = metadataResponseMessage[index..];
        index = 0;
        return BencodeDecoder.DecodeDictionary(ref metadata, ref index);
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
        var handshakePayload = BencodeDecoder.DecodeDictionary(ref payload, ref index);
        if (handshakePayload.TryGetValue("ut_metadata"u8.ToArray(), out var messageId) &&
            messageId is long messageIdLong)
        {
            extensionMessageId = (byte)messageIdLong;
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