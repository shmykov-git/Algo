namespace Font.Model.Fts;

public class FtFixed : Ft
{
    public override int Size => 4;
    public override FontType Type => FontType.Fixed;
}

public class FtFixed32 : FtFixed
{
    public override FontType Type => FontType.Fixed32;
}