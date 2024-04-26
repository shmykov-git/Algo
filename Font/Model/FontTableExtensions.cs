using Font.Extensions;

namespace Font.Model;

public static class FontTableExtensions
{

    private static (string tableName, string? rowExpression, string fieldName) ParsePath(FontTable table, string path)
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

    private static IEnumerable<FontTable> IterateBackAndUp(FontTable table)
    {
        if (table == null)
            yield break;

        foreach(var child in ((IEnumerable<FontTable>)table.ActiveTables).Reverse())
            yield return child;

        foreach(var item in IterateBackAndUp(table.ParentTable))
            yield return item;
    }

    public static FontTable FindTable(this FontTable table, string name) => IterateBackAndUp(table.ParentTable).First(t => t.Name == name);

    public static long FindRelatedOffset(this FontTable table, string? path)
    {
        if (path == null)
            return 0;
        
        (string relatedTableName, _) = path.SplitByTwo('.');
        var relatedTable = table.FindTable(relatedTableName);
        var value = table.FindValue(path);
        var relatedOffset = relatedTable.startPosition + value;
        
        return relatedOffset;
    }

    public static long FindValue(this FontTable table, string? path, long defaultValue = default)
    {
        if (path == null)
            return defaultValue;

        (string linkTableName, string? rowExpression, string fieldName) = ParsePath(table, path);
        var linkTable = table.FindTable(linkTableName);

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

    public static long GetRowsOffset(this FontTable table) => table.ParentTable.ActiveTables.SkipLast(1).Select(t => t.endPosition).LastOrDefault();

    public static bool CheckCondition(this FontTable table, string? condition)
    {
        if (condition == null)
            return true;

        (string link, string expectedValue) = condition.SplitByTwo('=');
        (string tableName, string field) = link.SplitByTwo('.');
        var linkTable = table.FindTable(tableName);
        var actualValue = linkTable.GetValue(0, linkTable.GetFieldIndex(field));

        return actualValue == expectedValue;
    }
}
