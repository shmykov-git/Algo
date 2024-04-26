
using Font.Extensions;

namespace Font.Model.Fts;

public class FtString : UFt
{
    public override bool IsSingleton => false;
    public override int Size => int.TryParse(field.Length!, out var value) ? value : (int)table.GetLongValue(field.Length!);
    public override FontType Type => FontType.String;
}

public class FtOffsetString : FtString
{
    public override FontType Type => FontType.OffsetString;

    //public override string GetStringValue(byte[] data) => GetArrayInfoValue(data).Replace("byte", "string");

    public override byte[] Read(BinaryReader reader)
    {
        var restore = reader.BaseStream.Position;
        var storageOffset = table.FindRelatedOffset(field.StorageOffset);
        var offset = table.GetLongValue(field.Offset!);        
        
        reader.Seek(storageOffset + offset, "<string>");
        var res = base.Read(reader);
        reader.Seek(restore, "</string>");

        return res;
    }
}