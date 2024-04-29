using Font.Exceptions;

namespace Font.Model.Fts.Arrays;

public class FtArray : UFt
{
    public override bool IsSingleton => false;
    public override int Size => int.TryParse(field.Length!, out var value) ? value : (int)table.GetLongValue(field.Length!);
    public override FontType Type => FontType.Array;
}

public class FtArrayX2 : FtArray
{
    public override int Size => 2 * (int)table.GetLongValue(field.Length!);
    public override FontType Type => FontType.ArrayX2;
}

public class FtArray_Glyph : FtArray
{
    public override int Size => (int)(table.GetLongValue(field.Length!) - (position - table.startPosition));
    public override FontType Type => FontType.ArrayGlyph;
}

public class FtArrayUInt16 : FtArrayX2
{
    public override FontType Type => FontType.ArrayUInt16;
}