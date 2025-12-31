namespace ERH.HeatScans.Reporting;

public class ReportImage(string id, int index)
{
    public string Id { get; private set; } = id;
    public int Index { get; private set; } = index;
    public string MimeType { get; set; }
    public long? Size { get; set; }
    public string ModifiedTime { get; set; }
    public string Name { get; set; }
    public Spots Spots { get; set; }
    public string Comments { get; set; }
    public bool ExcludeFromReport { get; set; } = false;

}
