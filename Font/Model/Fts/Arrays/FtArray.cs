using Font.Exceptions;

namespace Font.Model.Fts.Arrays;

public abstract class FtArray : UFt
{
    public override bool IsSingleton => false;
    public override int Size => throw new UnknownSizeException();
    public override FontType Type => throw new UnknownSizeException();
}

public class FtArray_Segment : FtArray
{
    public override int Size => (int)table.GetLongValue(field.LinkField!);
    public override FontType Type => FontType.ArraySegment;
}

public class FtArray_Glyph : FtArray
{
    public override int Size => (int)(table.GetLongValue(field.LinkField!) - (position - table.startPosition));
    public override FontType Type => FontType.ArrayGlyph;
}
