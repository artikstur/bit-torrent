using System.Text.Json;

namespace TorrentClient.Extensions;

public static class StringExtensions
{
    public static string CreatePeerAnswer(this string hash)
    {
        var peerMessage = new PeerMessage
        {
            Status = "found",
            Hash = hash
        };

        return JsonSerializer.Serialize(peerMessage);
    }

    public static string CreatePeerRequest(this string hash)
    {
        var peerMessage = new PeerMessage
        {
            Status = "search",
            Hash = hash
        };

        return JsonSerializer.Serialize(peerMessage);
    }
}