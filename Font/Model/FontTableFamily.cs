using Font.Extensions;

namespace Font.Model;

public class FontTableFamily
{
    public List<FontTable> Tables { get; } = new();

    private (string tableName, string? rowExpression, string fieldName) ParsePath(FontTable table, string path)
    {
        (string tablePart, string fieldName) = path.SplitByTwo('.');         

        if (tablePart.Contains("["))
        {
            (string tableName, string rowExpression) = tablePart.SplitByTwo('[', ']');
            rowExpression = rowExpression.Replace("{name}", table.Name);

            return (tableName, rowExpression, fieldName);
        }
        else
            return (tablePart, null, fieldName);
    }

    public FontTable GetTable(string name) => Tables.First(t => t.Name == name);

    public long FindValue(FontTable table, string? path, long defaultValue = default)
    {
        if (path == null)
            return defaultValue;

        (string linkTableName, string? rowExpression, string fieldName) = ParsePath(table, path);
        var linkTable = GetTable(linkTableName);

        var i = 0;
        if (rowExpression != null)
        {
            (string rowField, string rowValue) = rowExpression.SplitByTwo('=');
            i = linkTable.SearchRowIndex(rowField, rowValue);
        }

        var j = linkTable.GetFieldIndex(fieldName);
        var value = linkTable.GetLongValue(i, j);

        return value;
    }

    public bool CheckCondition(string? condition)
    {
        if (condition == null)
            return true;

        (string link, string expectedValue) = condition.SplitByTwo('=');
        (string tableName, string field) = link.SplitByTwo('.');
        var table = GetTable(tableName);
        var actualValue = table.GetValue(0, table.GetFieldIndex(field));

        return actualValue == expectedValue;
    }
}
