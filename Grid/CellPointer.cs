using System.Text.RegularExpressions;

namespace Lab1.Grid;

public partial class CellPointer
{
    private const string CellSeparator = "$";

    public CellPointer(int column, int row)
    {
        Column = column;
        Row = row;
    }

    public int Column { get; }
    public int Row { get; }

    public CellPointer(string rawValue)
    {
        if (string.IsNullOrEmpty(rawValue)) throw new ArgumentException("CellPointer cannot be null or empty");

        if (System.Text.Encoding.UTF8.GetByteCount(rawValue) != rawValue.Length)
        {
            throw new ArgumentException("CellPointer contains non-ASCII characters");
        }


        var firstSep = rawValue.IndexOf(CellSeparator, StringComparison.Ordinal);
        var secondSep = rawValue.IndexOf(CellSeparator, firstSep + 1, StringComparison.Ordinal);

        var columnSubstring = rawValue.Substring(firstSep + 1, secondSep - firstSep - 1);
        Column = ColumnToNumber(columnSubstring);
        Row = int.Parse(rawValue.Substring(secondSep + 1)) - 1;
    }

    public override string ToString()
    {
        return $"{CellSeparator}{NumberToColumn(Column)}{CellSeparator}{Row}";
    }

    public static int ColumnToNumber(string column)
    {
        return column.Aggregate(0, (current, c) => current * 26 + c - 'A' + 1) - 1;
    }

    public static string NumberToColumn(int columnNumber)
    {
        var column = string.Empty;

        if (columnNumber < 0) return column;

        do
        {
            var letter = (char)('A' + columnNumber % 26);
            column = letter + column;
            columnNumber /= 26;
            columnNumber--;
        } while (columnNumber > 0);

        return column;
    }

    public static List<CellPointer> FindPointers(string expression)
    {
        return CellPointerRegex()
            .Matches(expression)
            .Select(match => new CellPointer(match.Value))
            .ToList();
    }

    [GeneratedRegex(@"\$[A-Z]+\$\d+")]
    private static partial Regex CellPointerRegex();

    public override bool Equals(object? obj)
    {
        if (obj is CellPointer other)
        {
            return Column == other.Column && Row == other.Row;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Column, Row);
    }
}