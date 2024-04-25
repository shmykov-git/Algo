using System.Drawing;
using System.IO;
using Font.Exceptions;
using Model.Extensions;
using Model.Libraries;

namespace Font.Model;

public class FontTable
{
    public required string Name { get; set; }
    public string? Parent { get; set; }
    public required FontField[] Fields { get; set; }
    public string? Offset { get; set; }
    public string? RowsCount { get; set; }

    // <init>
    public FontTableFamily Family { get; set; }
    // </init>

    public void Read(BinaryReader reader)
    {
        startPosition = reader.BaseStream.Position;

        offsetValue = Family.FindValue(this, Offset);
        rowsCountValue = Family.FindValue(this, RowsCount, 1);

        var position = Parent == null ? startPosition : Family.GetTable(Parent).endPosition;
        reader.BaseStream.Seek(position + offsetValue, SeekOrigin.Begin);

        datas = new byte[rowsCountValue][][];

        for (var i = 0; i < rowsCountValue; i++)
        {
            datas[i] = new byte[Fields.Length][];

            for (var j = 0; j < Fields.Length; j++)
            {
                datas[i][j] = Fields[j].Ft.Read(reader);
            }
        }

        endPosition = reader.BaseStream.Position;
    }

    public string[][] values => datas.Select((row, i) => row.Select((_, j) => GetValue(i, j)).ToArray()).ToArray();
    public string[] compactValues => datas.Select((row, i) => row.Select((_, j) => $"({Fields[j].Name}: {GetValue(i, j)})").SJoin(" ")).ToArray();

    // <read>
    public byte[][][] datas;

    public long startPosition;
    public long endPosition;
    public long rowsCountValue;
    public long offsetValue;
    // </read>

    public int GetFieldIndex(string fieldName) => Fields.Select((f, j) => (f, j)).First(v => v.f.Name == fieldName).j;
    public int SearchRowIndex(string fieldName, string value)
    {
        var j = GetFieldIndex(fieldName);
        return Enumerable.Range(0, (int)rowsCountValue).Where(i => GetStringValue(i, j) == value).First();
    }

    public string GetValue(int i, int j)
    {
        if (datas == null)
            throw new NotReadYetException();

        return Fields[j].Ft!.GetValue(datas[i][j]);
    }

    public long GetLongValue(int i, int j)
    {
        if (datas == null)
            throw new NotReadYetException();

        return Fields[j].Ft!.GetLongValue(datas[i][j]);
    }

    public string GetStringValue(int i, int j)
    {
        if (datas == null)
            throw new NotReadYetException();

        return Fields[j].Ft!.GetStringValue(datas[i][j]);
    }
}

public class FontField
{
    public string? Name { get; set; }
    public required FontType Type { get; set; }
    public Ft? Ft { get; set; } = null;
}
