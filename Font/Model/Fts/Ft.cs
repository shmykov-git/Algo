namespace Font.Model.Fts;

public abstract class Ft
{
    public virtual bool IsSingleton { get; } = true;
    public virtual bool Signed { get; } = true;
    public abstract int Size { get; }
    public abstract FontType Type { get; }

    // <not_singleton>
    public FontTable table;
    public FontField field;
    protected long position = -1;
    // </not_singleton>


    private static FontType[] stringTypes = [FontType.Tag, FontType.String, FontType.OffsetString];
    public static FontType[] fixedTypes = [FontType.Fixed];

    public string GetValue(byte[] data)
    {
        if (stringTypes.Contains(Type))
            return GetStringValue(data);

        if (fixedTypes.Contains(Type))
            return GetFixedValue(data).ToString();

        if (!IsSingleton)
            return GetArrayInfoValue(data);

        return GetLongValue(data).ToString();
    }

    public string GetArrayInfoValue(byte[] data) => $"byte[{data.Length}]";

    public float GetFixedValue(byte[] data)
    {
        if (data.Length != 4)
            throw new NotImplementedException("only 4");

        var (decData, fracData) = (data.Take(2).ToArray(), data.Skip(2).ToArray());

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(decData);
            Array.Reverse(fracData);
        }

        var dec = BitConverter.ToInt16(decData);
        var frac = BitConverter.ToUInt16(fracData);

        return dec + frac / 65535f;
    }

    public long GetLongValue(byte[] data)
    {
        if (BitConverter.IsLittleEndian)
            data = data.Reverse().ToArray();

        return (Signed, Size) switch
        {
            (false, 1) => data[0],
            (false, 2) => BitConverter.ToUInt16(data),
            (false, 4) => BitConverter.ToUInt32(data),
            (false, 8) => (long)BitConverter.ToUInt64(data),
            (true, 1) => (sbyte)data[0],
            (true, 2) => BitConverter.ToInt16(data),
            (true, 4) => BitConverter.ToInt32(data),
            (true, 8) => BitConverter.ToInt64(data),

            _ => throw new NotImplementedException(),
        };
    }

    public virtual string GetStringValue(byte[] data)
    {
        return new string(data.Select(d => (char)d).ToArray())
            .Replace("\0", string.Empty)
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Trim();
    }

    public virtual byte[] Read(BinaryReader reader)
    {
        if (!IsSingleton)
            position = reader.BaseStream.Position;

        var data = new byte[Size];
        reader.Read(data, 0, Size);

        return data;
    }

    public void Write(BinaryWriter writer, byte[] data)
    {
        writer.Write(data, 0, Size);
    }
}


public abstract class UFt : Ft
{
    public override bool Signed => false;
}

public class Ftuint8 : UFt
{
    public override int Size => 1;
    public override FontType Type => FontType.uint8;
}

public class Ftuint16 : UFt
{
    public override int Size => 2;
    public override FontType Type => FontType.uint16;
}

public class Ftuint24 : UFt
{
    public override int Size => 3;
    public override FontType Type => FontType.uint24;
}

public class Ftuint32 : UFt
{
    public override int Size => 4;
    public override FontType Type => FontType.uint32;
}

public class Ftint8 : Ft
{
    public override int Size => 1;
    public override FontType Type => FontType.int8;
}

public class Ftint16 : Ft
{
    public override int Size => 2;
    public override FontType Type => FontType.int16;
}

public class Ftint32 : Ft
{
    public override int Size => 4;
    public override FontType Type => FontType.int32;
}

public class FtFWORD : Ftint16
{
    public override FontType Type => FontType.FWORD;
}

public class FtUFWORD : Ftuint16
{
    public override FontType Type => FontType.UFWORD;
}

public class FtF2DOT14 : Ftint16
{
    public override FontType Type => FontType.F2DOT14;
}

public class LONGDATETIME : Ft
{
    public override int Size => 8;
    public override FontType Type => FontType.LONGDATETIME;
}

public class FtTag : Ft
{
    public override int Size => 4;
    public override FontType Type => FontType.Tag;
}
