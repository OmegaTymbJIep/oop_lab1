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
        /// <param name="pointer">The cell pointer to retrieve the data from.</param>
        /// <returns>
        /// The data stored at the specified cell as a string, or <c>null</c> if the cell is empty.
        /// </returns>
        public string? GetCellData(CellPointer pointer);
    }
}