namespace BatchProcessor.Types;

public class BatchRequest
{
    public List<string> IpAddresses { get; set; } = [];
}

public class Batch
{
    public Guid BatchId { get; set; }
    public List<string> IpAddresses { get; set; }
    public StatusEnum Status { get; set; } = StatusEnum.Pending;
    public int ProcessedCount { get; set; } = 0;
    public int TotalCount { get; set; } = 0;
    public List<string> Results { get; set; } = [];
}

public enum StatusEnum
{
    Pending,
    InProgress,
    Completed,
    Failed
}