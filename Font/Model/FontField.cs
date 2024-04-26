using Font.Model.Fts;

namespace Font.Model;

public class FontField
{
    public string? Name { get; set; }
    public required FontType Type { get; set; }
    public string? Length { get; set; }
    public string? StorageOffset { get; set; }
    public string? Offset { get; set; }
    public Ft? Ft { get; set; } = null;
}
