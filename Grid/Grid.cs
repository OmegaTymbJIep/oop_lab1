using System.Collections;
using System.Text.Json;

namespace Lab1.Grid;

/// Represents a two-dimensional, dynamically resizable grid structure that stores data in a row and column format.
public class Grid : IGrid, IEnumerable<(int Row, int Column, string Value)>
{
    /// Inner grid representation: row -> (col -> value)
    private readonly Dictionary<int, Dictionary<int, string>> _inner;

    /// Initializes a new instance of the <see cref="Grid"/> class
    public Grid()
    {
        _inner = new Dictionary<int, Dictionary<int, string>>();
    }

    /// Initializes a new instance of the <see cref="Grid"/> class, allocating the specified number of rows and columns.
    public Grid(int rows, int columns)
    {
        _inner = new Dictionary<int, Dictionary<int, string>>(rows);

        for (var i = 0; i < rows; i++)
        {
            _inner[i] = new Dictionary<int, string>(columns);
        }
    }

    /// Gets or sets the data for a specified cell in the grid.
    /// <param name="row">The zero-based index of the row.</param>
    /// <param name="col">The zero-based index of the column.</param>
    /// <returns>The data stored at the specified cell as a string, or <c>null</c> if the cell is empty.</returns>
    public string? this[int row, int col]
    {
        get => !_inner.TryGetValue(row, out var value) ? null : value.GetValueOrDefault(col);
        set
        {
            if (!_inner.ContainsKey(row))
            {
                _inner[row] = new Dictionary<int, string>();
            }

            if (string.IsNullOrEmpty(value))
            {
                _inner[row].Remove(col);
                return;
            }

            _inner[row][col] = value;
        }
    }

    /// Writes the JSON grid representation to a stream.
    /// <param name="stream">The stream where to write the JSON grid representation.</param>
    public async void WriteToJsonStreamAsync(Stream stream)
    {
        var nonEmptyCells = new Dictionary<int, Dictionary<int, string>>();

        foreach (var row in _inner)
        {
            var nonEmptyColumns = new Dictionary<int, string>();
            foreach (var col in row.Value.Where(col => !string.IsNullOrEmpty(col.Value)))
            {
                nonEmptyColumns[col.Key] = col.Value;
            }

            if (nonEmptyColumns.Count > 0)
            {
                nonEmptyCells[row.Key] = nonEmptyColumns;
            }
        }

        await JsonSerializer.SerializeAsync(stream, nonEmptyCells, new JsonSerializerOptions { WriteIndented = true });
    }

    /// Reads JSON grid representtation from a stream and loads it into the grid.
    /// <param name="stream">The stream where to read the JSON grid representation.</param>
    public async Task ReadFromJsonStreamAsync(Stream stream)
    {
        var data = await JsonSerializer.DeserializeAsync<Dictionary<int, Dictionary<int, string>>>(stream);

        if (data == null)
        {
            return;
        }

        foreach (var row in data)
        {
            foreach (var col in row.Value)
            {
                this[row.Key, col.Key] = col.Value;
            }
        }
    }

    // IEnumerable implementation

    public IEnumerator<(int Row, int Column, string Value)> GetEnumerator()
    {
        foreach (var rowPair in _inner)
        {
            var row = rowPair.Key;
            foreach (var colPair in rowPair.Value)
            {
                int column = colPair.Key;
                string value = colPair.Value;
                yield return (row, column, value);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // IGrid implementation

    public int Rows()
    {
        return _inner.Count;
    }

    public int Columns()
    {
        return _inner.Values.Max(row => row.Keys.Max());
    }

    public string? GetCellData(int row, int column)
    {
        return this[row, column];
    }
}