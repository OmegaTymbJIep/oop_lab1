namespace Lab1.Grid
{
    /// Represents a grid structure that provides access to its rows, columns, and cell data.
    public interface IGrid
    {
        /// Gets the total number of rows in the grid.
        /// <returns>The number of rows in the grid.</returns>
        public int Rows();

        /// Gets the total number of columns in the grid.
        /// <returns>The number of columns in the grid.</returns>
        public int Columns();

        /// Retrieves the data at the specified cell in the grid.
        /// <param name="row">The zero-based index of the row.</param>
        /// <param name="column">The zero-based index of the column.</param>
        /// <returns>
        /// The data stored at the specified cell as a string, or <c>null</c> if the cell is empty.
        /// </returns>
        public string? GetCellData(int row, int column);
    }
}