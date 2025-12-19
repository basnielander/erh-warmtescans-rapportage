namespace ERH.HeatScans.Reporting.Server.Models;

public record GoogleDriveItem
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string MimeType { get; init; } = string.Empty;
    public bool IsFolder { get; init; }
    public DateTime? ModifiedTime { get; init; }
    public long? Size { get; init; }
    public List<GoogleDriveItem> Children { get; set; } = new();
}
