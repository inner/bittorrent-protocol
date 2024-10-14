using System.Net;
using System.Text;
using System.Web;
using codecrafters_bittorrent.Bencoding;
using codecrafters_bittorrent.Connection;

namespace codecrafters_bittorrent.Extensions;

public static class TrackerExtensions
{
    public static string GetTrackerUrl(this string magnet)
    {
        return WebUtility.UrlDecode(magnet.Split("&tr=")[1]);
    }
    
    public static byte[] GetInfoHash(this string magnet)
    {
        var infoHash = magnet.Split("&dn=")[0].Split(":")[3];
        
        var infoHashBytes = new byte[infoHash.Length / 2];
        for (var i = 0; i < infoHashBytes.Length; i++)
        {
            infoHashBytes[i] = byte.Parse(infoHash.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
        }

        return infoHashBytes;
    }
    
    public static async Task<List<Peer>> DiscoverPeers(string trackerUrl, byte[] infoHash, long leftLength)
    {
        var peersList = new List<Peer>();
        var httpClient = new HttpClient();
        var requestUri = new Uri(trackerUrl);
        var baseUri = requestUri.ToString();
        
        var queryParameters = new Dictionary<string, string>
        {
            { "info_hash", HttpUtility.UrlEncode(infoHash) },
            { "peer_id", GeneratePeerId() },
            { "port", "6881" },
            { "uploaded", "0" },
            { "downloaded", "0" },
            { "left", leftLength.ToString() },
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

                peersList.Add(new Peer(ip.ToString(), port));
            }
        }
        else
        {
            Console.WriteLine("The 'peers' key was not found in the dictionary.");
        }
        
        return peersList;
    }
    
    public static string GeneratePeerId()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 20)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}