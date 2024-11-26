using System.Collections;
using System.Text.Json;

namespace Lab1.Core.Grid;

/// Represents a two-dimensional, dynamically resizable grid structure that stores data in a row and column format.
public class Grid : IGrid
{
    /// Inner grid representation: row -> (col -> value)
    private readonly Dictionary<int, Dictionary<int, string>> _inner;

    private int _nbColumns;
    private int _nbRows;

    /// The mapping of which cells reference which other cells.
    private readonly Dictionary<CellPointer, List<CellPointer>> _references = new();

    /// The mapping of which cells are referenced by which other cells.
    private readonly Dictionary<CellPointer, List<CellPointer>> _dependents = new();

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
    private string? this[int row, int col]
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

            if (col >= _nbColumns)
            {
                _nbColumns = col + 1;
            }

            if (row >= _nbRows)
            {
                _nbRows = row + 1;
            }

            _inner[row][col] = value;
        }
    }

    // IEnumerable implementation

    public IEnumerator<(CellPointer pointer, string Value)> GetEnumerator()
    {
        foreach (var (row, columns) in _inner)
        {
            foreach (var (column, value) in columns)
            {
                yield return (new CellPointer(column, row), value);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // IGrid implementation

    public int Rows()
    {
        return _nbRows;
    }

    public int Columns()
    {
        return _nbColumns;
    }

    public string GetCellData(CellPointer pointer)
    {
        var result = this[pointer.Row, pointer.Column];
        return string.IsNullOrEmpty(result) ? string.Empty : result;
    }

    public async Task WriteToJsonStreamAsync(Stream stream)
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

    public async Task ReadFromJsonStreamAsync(Stream stream)
    {
        var data = await JsonSerializer.DeserializeAsync<Dictionary<int, Dictionary<int, string>>>(stream);

        if (data == null)
        {
            return;
        }

        _references.Clear();
        _dependents.Clear();
        _inner.Clear();
        _nbColumns = 0;
        _nbRows = 0;

        foreach (var (row, cols) in data)
        {
            foreach (var (col, value) in cols)
            {
                var pointer = new CellPointer(col, row);

                this[row, col] = value;

                var usedInCells = CellPointer.FindPointers(value);
                _references[pointer] = usedInCells;
                foreach (var usedCell in usedInCells)
                {
                    if (!_dependents.ContainsKey(usedCell))
                    {
                        _dependents[usedCell] = [];
                    }
                    _dependents[usedCell].Add(pointer);
                }
            }
        }
    }

    public List<CellPointer> UpdateCell(CellPointer pointer, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            value = string.Empty;

            if (_references.TryGetValue(pointer, out var usedCells))
            {
                foreach (var usedCell in usedCells.Where(usedCell => !Equals(usedCell, pointer)))
                {
                    _dependents[usedCell].Remove(pointer);
                }

                _references.Remove(pointer);
            }
        }

        this[pointer.Row, pointer.Column] = value;

        var newReferences = CellPointer.FindPointers(value);
        newReferences.Remove(pointer);

        var oldReferences = _references.GetValueOrDefault(pointer, []);
        foreach (var oldReference in oldReferences)
        {
            if (!newReferences.Contains(oldReference))
            {
                _dependents[oldReference].Remove(pointer);
            }
        }

        _references[pointer] = newReferences;

        foreach (var reference in newReferences)
        {
            if (!_dependents.ContainsKey(reference))
            {
                _dependents[reference] = [];
            }

            if (!_dependents[reference].Contains(pointer))
            {
                _dependents[reference].Add(pointer);
            }
        }

        return _dependents.GetValueOrDefault(pointer, []);
    }

    public List<CellPointer> ClearCell(CellPointer pointer)
    {
        return UpdateCell(pointer, string.Empty);
    }

    public List<CellPointer> GetDependents(CellPointer pointer)
    {
        return _dependents.GetValueOrDefault(pointer, []);
    }
}