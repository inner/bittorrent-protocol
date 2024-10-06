using System.Net;
using System.Text;
using System.Web;

namespace codecrafters_bittorrent;

public class Torrent(byte[] torrentBytes)
{
    private Dictionary<byte[], object> TorrentDictionary => torrentBytes.ToDictionary();
    private Dictionary<byte[], object> InfoDictionary => TorrentDictionary.ToInfoDictionary();
    private byte[] PiecesInBytes => InfoDictionary.ToPiecesInBytes();
    public byte[] InfoHash => InfoDictionary.ToInfoHash();
    public string InfoHashHex => InfoHash.ToInfoHashHex();
    public string TrackerUrl => TorrentDictionary.ToTrackerUrl();
    public string Name => InfoDictionary.ToName();
    public long Length => InfoDictionary.ToLength();
    public long PieceLength => InfoDictionary.ToPieceLength();
    public List<string> PieceHashes => PiecesInBytes.ToPieceHashes();
    
    public async Task<List<string>> DiscoverPeers()
    {
        var peersList = new List<string>();
        var httpClient = new HttpClient();
        var requestUri = new Uri(TrackerUrl);
        var baseUri = requestUri.ToString();
        
        var queryParameters = new Dictionary<string, string>
        {
            { "info_hash", HttpUtility.UrlEncode(InfoHash) },
            { "peer_id", "00112233445566778899" },
            { "port", "6881" },
            { "uploaded", "0" },
            { "downloaded", "0" },
            { "left", Length.ToString() },
            { "compact", "1" }
        };
        
        var queryStringBuilder = new StringBuilder();
        foreach (var kvp in queryParameters)
        {
            if (queryStringBuilder.Length > 0)
            {
                queryStringBuilder.Append('&');
            }

            queryStringBuilder.Append($"{kvp.Key}={kvp.Value}");
        }

        var fullUri = $"{baseUri}?{queryStringBuilder}";

        var response = await httpClient.GetAsync(fullUri);
        var responseContent = await response.Content.ReadAsByteArrayAsync();
        
        var index = 0;
        var dict = BencodeDecoder.DecodeDictionary(ref responseContent, ref index);

        if (dict.TryGetValue("peers"u8.ToArray(), out var peersValue))
        {
            var peers = (byte[])peersValue;
            var peersCount = peers.Length / 6;

            for (var i = 0; i < peersCount; i++)
            {
                var ipBytes = peers[(i * 6)..(i * 6 + 4)];
                var portBytes = peers[(i * 6 + 4)..(i * 6 + 6)];

                var ip = new IPAddress(ipBytes);
                var port = BitConverter.ToUInt16(portBytes.Reverse().ToArray(), 0);

                peersList.Add($"{ip}:{port}");
            }
        }
        else
        {
            Console.WriteLine("The 'peers' key was not found in the dictionary.");
        }
        
        return peersList;
    }
}