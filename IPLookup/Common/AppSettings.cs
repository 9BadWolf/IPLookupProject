using System.ComponentModel.DataAnnotations;

namespace IPLookup.Common;

public class IpStack
{
    internal const string ConfigurationSectionName = "IpStack";
    public string BaseUrl { get; init; }
    [Required]
    [MinLength(1)]
    public string AccessKey { get; init; }
}