using System.Diagnostics;
using Font.Exceptions;
using Model.Extensions;

namespace Font.Model;

public class FontTable
{
    public required string Name { get; set; }
    public string? OffsetTable { get; set; }
    public string? Condition { get; set; }
    public required FontField[] Fields { get; set; }
    public string? Offset { get; set; }
    public string? RowsCount { get; set; }              // ссылка на таблицу где указано число строк в этой таблице
    
    public bool Break {  get; set; }
    
    public FontTable? ParentTable { get; set; }
    
    public FontTable[] Tables { get; set; }
    public List<FontTable> ActiveTables { get; set; }

    public int Level => ParentTable == null ? 0 : ParentTable.Level + 1;
    public string FullName
    {
        get
        {
            if (ParentTable == null)
                return Name;

            var fullName = ParentTable.FullName;

            if (fullName == null)
                return Name;

            return $"{fullName}.{Name}";
        }
    }

    //public long rowsCount => RowsCount == null ? rowsCountValue : this.FindValue(RowsCount);


    private void SetTablePosition(BinaryReader reader)
    {
        var offset = this.FindValue(Offset);

        var offsetTableOffset = OffsetTable == null ? 0 : this.FindTable(OffsetTable).startPosition;
        var globalOffset = offset + offsetTableOffset;

        if (globalOffset > 0)
            Seek(reader, globalOffset, "new table");
    }

    private void Seek(BinaryReader reader, long position, string desc)
    {
        //Debug.WriteLine($"Position={position} ({desc})");
        reader.BaseStream.Seek(position, SeekOrigin.Begin);
    }

    public void Read(BinaryReader reader, int parentRowNumber, Action<int> readRowData)
    {
        if (Break) Debugger.Break();

        if (!this.CheckCondition(Condition))
            return;

        if (parentRowNumber == 0)
            SetTablePosition(reader);

        var rowsCountValue = this.FindValue(RowsCount, 1);
        startPosition = reader.BaseStream.Position;
        datas = new byte[rowsCountValue][][];
        //Debug.WriteLine($"DataPos={reader.BaseStream.Position}");
        for (var i = 0; i < rowsCountValue; i++)
        {
            datas[i] = new byte[Fields.Length][];

            for (var j = 0; j < Fields.Length; j++)
            {
                datas[i][j] = Fields[j].Ft.Read(reader);
            }

            var endRowPosition = reader.BaseStream.Position;            
            readRowData(i);
            
            if (IsTable)
                Seek(reader, endRowPosition, $"end of {Name} row");
        }

        endPosition = reader.BaseStream.Position;
    }

    public bool IsTable => Fields != null && Fields.Length > 0;

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
    //public long rowsCountValue;
    //public long offsetValue;
    // </read>

    public int GetFieldIndex(string fieldName) => Fields.Select((f, j) => (f, j)).First(v => v.f.Name == fieldName).j;
    public int SearchRowIndex(string fieldName, string value)
    {
        var j = GetFieldIndex(fieldName);
        return Enumerable.Range(0, datas.Length).Where(i => GetStringValue(i, j) == value).First();
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
