namespace Lab1.Core.Grid
{
    /// Represents a grid structure that provides access to its rows, columns, and cell data.
    public interface IGrid : IEnumerable<(CellPointer pointer, string Value)>
    {
        /// <summary>
        /// Gets the total number of rows in the grid.
        /// </summary>
        /// <returns>The number of rows in the grid.</returns>
        public int Rows();

        /// <summary>
        /// Gets the total number of columns in the grid.
        /// </summary>
        /// <returns>The number of columns in the grid.</returns>
        public int Columns();

        /// <summary>
        /// Retrieves the data at the specified cell in the grid.
        /// </summary>
        /// <param name="pointer">The cell pointer to retrieve the data from.</param>
        /// <returns>
        /// The data stored at the specified cell as a string, or <c>null</c> if the cell is empty.
        /// </returns>
        public string GetCellData(CellPointer pointer);

        /// <summary>
        /// Writes the JSON grid representation to a stream.
        /// <param name="stream">The stream where to write the JSON grid representation.</param>
        public Task WriteToJsonStreamAsync(Stream stream);

        /// Reads JSON grid representtation from a stream and loads it into the grid.
        /// <param name="stream">The stream where to read the JSON grid representation.</param>
        public Task ReadFromJsonStreamAsync(Stream stream);

        /// <summary>
        /// Updates the cell at the specified pointer with the given value.
        /// </summary>
        /// <param name="pointer">The pointer to the cell to update.</param>
        /// <param name="value">The new value to set for the cell.</param>
        /// <returns>A list of pointers to cells that should be reevaluated.</returns>
        public List<CellPointer> UpdateCell(CellPointer pointer, string value);

        /// <summary>
        /// Clears the cell at the specified pointer.
        /// </summary>
        /// <param name="pointer">The pointer to the cell to update.</param>
        /// <returns>A list of pointers to cells that should be reevaluated.</returns>
        public List<CellPointer> ClearCell(CellPointer pointer);

        /// <summary>
        /// Retrieves a list of pointers to cells that depend on the specified cell.
        /// </summary>
        /// <param name="pointer">The pointer to the cell to retrieve dependents for.</param>
        /// <returns>A list of pointers to cells that depend on the specified cell.</returns>
        public List<CellPointer> GetDependents(CellPointer pointer);
    }
}