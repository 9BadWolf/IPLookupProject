using System.Text.Json.Serialization;

namespace IPLookup.Types;

public class Error
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("info")]
    public string Info { get; set; }
}

public class IpStackError
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("error")]
    public Error Error { get; set; }
}