
namespace Font.Model.Fts;

public class FtIgnored : Ft
{
    public override int Size => 0;
    public override FontType Type => FontType.Ignored;
    public override byte[] Read(BinaryReader reader) => new byte[0];
}

public class FtStateHeader : FtIgnored
{
    public override int Size => 0;
    public override FontType Type => FontType.StateHeader;
}