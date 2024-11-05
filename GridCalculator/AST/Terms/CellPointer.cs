namespace Lab1.GridCalculator.AST.Terms;

public class CellPointer : Term
{
    private const string CellSeparator = "$";

    public CellPointer(int column, int row)
    {
        Column = column;
        Row = row;
    }

    public int Column { get; protected set; }
    public int Row { get; protected set; }

    public CellPointer(string rawValue)
    {
        if (string.IsNullOrEmpty(rawValue)) throw new ArgumentException("CellPointer cannot be null or empty");

        if (System.Text.Encoding.UTF8.GetByteCount(rawValue) != rawValue.Length)
        {
            throw new ArgumentException("CellPointer contains non-ASCII characters");
        }


        int firstSep = rawValue.IndexOf(CellSeparator, StringComparison.Ordinal);
        int secondSep = rawValue.IndexOf(CellSeparator, firstSep + 1, StringComparison.Ordinal);

        var columnSubstring = rawValue.Substring(firstSep + 1, secondSep - firstSep - 1);
        Column = ColumnToNumber(columnSubstring);
        Row = int.Parse(rawValue.Substring(secondSep + 1));
    }

    public override string ToString()
    {
        return $"{CellSeparator}{NumberToColumn(Column)}{CellSeparator}{Row}";
    }

    private static int ColumnToNumber(string column)
    {
        return column.Aggregate(0, (current, с) => current * 26 + (с - 'A' + 1));
    }

    private static string NumberToColumn(int columnNumber)
    {
        var column = string.Empty;

        while (columnNumber > 0)
        {
            columnNumber--;
            var letter = (char)('A' + (columnNumber % 26));
            column = letter + column;
            columnNumber /= 26;
        }

        return column;
    }
}