using Font.Exceptions;
using Model.Extensions;

namespace Font.Model;

public class FontTable
{
    public required string Name { get; set; }
    public string? Parent { get; set; }
    public string? Condition { get; set; }
    public required FontField[] Fields { get; set; }
    public string? Offset { get; set; }
    public string? RowsCount { get; set; }
    public string? ReadIterationField { get; set; }
    public int TableIndex { get; set; } = 0;

    public int ReadIterationCount => ReadIterationField == null || !IsRead ? 1 : GetIntValue(ReadIterationField);

    // <init>
    public FontTableFamily Family { get; set; }
    // </init>

    public void Read(BinaryReader reader)
    {
        if (!Family.CheckCondition(Condition))
            return;

        offsetValue = Family.FindValue(this, Offset);
        rowsCountValue = Family.FindValue(this, RowsCount, 1);

        var parentOffset = Parent == null ? 0 : Family.GetTable(Parent).startPosition;
        var globalOffset = offsetValue + parentOffset;

        if (globalOffset > 0)
            reader.BaseStream.Seek(globalOffset, SeekOrigin.Begin);

        startPosition = reader.BaseStream.Position;

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

    public string[][] values => IsRead 
        ? datas.Select((row, i) => row.Select((_, j) => GetValue(i, j)).ToArray()).ToArray()
        : new string[0][];

    public string[] compactValues => IsRead
        ? datas.Select((row, i) => row.Select((_, j) => $"({Fields[j].Name}: {GetValue(i, j)})").SJoin(" ")).ToArray()
        : new string[0];

    // <read>
    public bool IsRead => datas != null;
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

    public int GetIntValue(string fieldName) => (int)GetLongValue(fieldName);

    public long GetLongValue(string fieldName)
    {
        var j = GetFieldIndex(fieldName);
        return GetLongValue(0, j);
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
