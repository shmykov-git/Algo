namespace Font.Model;

public class FontTableFamily
{
    public FontTable[] Tables { get; set; }

    private (string tableName, string? rowExpression, string fieldName) ParsePath(FontTable table, string path)
    {
        var parts = path.Split('.');
        (string tablePart, string fieldName) = (parts[0], parts[1]);         

        if (tablePart.Contains("["))
        {
            var tParts = tablePart.Split(['[', ']']);
            (string tableName, string rowExpression) = (tParts[0], tParts[1]);
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
            var rowParts = rowExpression.Split('=');
            (string rowField, string rowValue) = (rowParts[0], rowParts[1]);
            i = linkTable.SearchRowIndex(rowField, rowValue);
        }

        var j = linkTable.GetFieldIndex(fieldName);
        var value = linkTable.GetLongValue(i, j);

        return value;
    }
}
