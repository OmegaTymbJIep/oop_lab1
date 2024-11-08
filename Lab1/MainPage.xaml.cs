using System.Globalization;
using Lab1.Core.Grid;
using Lab1.Core.Services;
using Lab1.Services;
using MauiGrid = Microsoft.Maui.Controls.Grid;
using ExprGrid = Lab1.Core.Grid.Grid;

namespace Lab1;

public partial class MainPage
{
    private const string DefaultGridSaveFileName = "gridsave.json";

    private const int MinColumnsNumber = 22;
    private const int MinRowsNumber = 22;

    private readonly IGrid _exprGrid;
    private readonly Core.GridCalculator.GridCalculator _gridCalculator;

    private readonly IFileSavePicker _fileSavePicker = new FileSavePicker();

    internal static class Pallete
    {
        public static readonly Color ErrorBackground = Color.FromRgb(75, 49, 55);
        public static readonly Color ErrorText = Color.FromRgb(254, 148, 148);
        public static readonly Color DefaultBackground = Color.FromRgb(31, 31, 31);
    }

    public MainPage()
    {
        InitializeComponent();
        CreateGrid();

        _exprGrid = new ExprGrid(MinRowsNumber, MinColumnsNumber);
        _gridCalculator = new Core.GridCalculator.GridCalculator(_exprGrid);
    }

    private void CreateGrid(int nbColumns = MinColumnsNumber, int nbRows = MinRowsNumber)
    {
        nbColumns = Math.Max(nbColumns, MinColumnsNumber);
        nbRows = Math.Max(nbRows, MinRowsNumber);

        AddColumnsAndColumnLabels(nbColumns);
        AddRowsAndCellEntries(nbRows, nbColumns);
    }

    private void AddColumnsAndColumnLabels(int nbColumns)
    {
        for (var col = 0; col <= nbColumns; col++)
        {
            Grid.ColumnDefinitions.Add(new ColumnDefinition());

            var label = NewLabel(CellPointer.NumberToColumn(col - 1));

            // Double click recognition for row width adjustment.
            var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            var column = col;
            tapGesture.Tapped += (_, _) => AdjustColumnWidth(column-1);
            label.GestureRecognizers.Add(tapGesture);

            MauiGrid.SetRow(label, 0);
            MauiGrid.SetColumn(label, col);
            Grid.Children.Add(label);
        }
    }

    private void AddRowsAndCellEntries(int nbRows, int nbColumns)
    {
        Grid.RowDefinitions.Add(new RowDefinition());

        for (var row = 1; row <= nbRows; row++)
        {
            Grid.RowDefinitions.Add(new RowDefinition());

            var label = NewLabel(row.ToString());

            MauiGrid.SetRow(label, row);
            MauiGrid.SetColumn(label, 0);
            Grid.Children.Add(label);

            for (var col = 1; col <= nbColumns; col++)
            {
                var entry = NewEmptyEntry();

                MauiGrid.SetRow(entry, row);
                MauiGrid.SetColumn(entry, col);
                Grid.Children.Add(entry);
            }
        }
    }

    private void AdjustColumnWidth(int columnIndex)
    {
        double maxWidth = 0;

        // Find the maximum width of all entries in the Column.
        for (var rowIndex = 1; rowIndex <= Grid.RowDefinitions.Count; rowIndex++)
        {
            if (Grid.Children.FirstOrDefault(child =>
                    Grid.GetRow(child) == rowIndex && Grid.GetColumn(child) == columnIndex
                ) is not Entry entry) continue;

            var entryWidth = entry.Measure(double.PositiveInfinity, entry.Height).Request.Width;

            maxWidth = Math.Max(maxWidth, entryWidth);
        }

        Grid.ColumnDefinitions[columnIndex].Width = new GridLength(maxWidth, GridUnitType.Absolute);

        // Trigger a layout update to immediately apply the new widths.
        Grid.Dispatcher.Dispatch(() => {
            Grid.IsVisible = false;
            Grid.IsVisible = true;
        });
    }

    private void Entry_Unfocused(object sender, FocusEventArgs e)
    {
        var entry = (Entry)sender;

        var column = MauiGrid.GetColumn(entry) - 1;
        var row = MauiGrid.GetRow(entry) - 1;
        var pointer = new CellPointer(column, row);

        if (entry.Text.Contains(pointer.ToString()))
        {
            entry.Text = _exprGrid.GetCellData(pointer);
            DisplayAlert("Error", "Self-reference detected", "OK");
            return;
        }

        var updatedCells = _exprGrid.UpdateCell(pointer, entry.Text);

        UpdateCellView(pointer, entry.Text);
        RecUpdateViewFromList(updatedCells);
    }

    private void Entry_Focused(object sender, FocusEventArgs e)
    {
        var entry = (Entry)sender;

        var column = MauiGrid.GetColumn(entry) - 1;
        var row = MauiGrid.GetRow(entry) - 1;
        var pointer = new CellPointer(column, row);

        entry.Text = _exprGrid.GetCellData(pointer);
    }

    private void CalculateButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var result = _gridCalculator.Evaluate(TextInput.Text);
            DisplayAlert("Result", result.ToString(CultureInfo.InvariantCulture), "OK");
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void SaveButton_Clicked(object sender, EventArgs e)
    {
        var filePath = await _fileSavePicker.PickAsync(DefaultGridSaveFileName);
        if (string.IsNullOrEmpty(filePath)) return;

        var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
        await _exprGrid.WriteToJsonStreamAsync(fileStream);

        fileStream.Close();
    }

    private async void ReadButton_Clicked(object sender, EventArgs e)
    {
        var fileResult = await FilePicker.PickAsync();

        if (fileResult == null) return;
        if (string.IsNullOrEmpty(fileResult.FullPath)) return;

        var fileStream = await fileResult.OpenReadAsync();
        await _exprGrid.ReadFromJsonStreamAsync(fileStream);

        fileStream.Close();

        UpdateViewFromExprGrid();
    }

    private async void ExitButton_Clicked(object sender, EventArgs e)
    {
        var answer = await DisplayAlert("Підтвердження", "Ви дійсно хочете вийти?", "Так", "Ні");

        if (answer)
        {
            Environment.Exit(0);
        }
    }

    private async void HelpButton_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Довідка", "Лабораторна робота 1. Студента Левочко Антон", "ОК");
    }

    private void DeleteRowButton_Clicked(object sender, EventArgs e)
    {
        if (Grid.RowDefinitions.Count <= MinRowsNumber + 1) return;

        var lastRowIndex = Grid.RowDefinitions.Count - 1;

        Grid.RowDefinitions.RemoveAt(lastRowIndex);

        for (var col = 0; col < Grid.ColumnDefinitions.Count; col++)
        {
            var element = Grid.Children
                .FirstOrDefault(child => Grid.GetRow(child) == lastRowIndex && Grid.GetColumn(child) == col);

            var updatedCells =  _exprGrid.ClearCell(new CellPointer(col, lastRowIndex - 1));
            RecUpdateViewFromList(updatedCells);

            if (element != null)
            {
                Grid.Children.Remove(element);
            }
        }
    }

    private void DeleteColumnButton_Clicked(object sender, EventArgs e)
    {
        if (Grid.ColumnDefinitions.Count <= MinColumnsNumber + 1) return;

        var lastColumnIndex = Grid.ColumnDefinitions.Count - 1;

        Grid.ColumnDefinitions.RemoveAt(lastColumnIndex);

        for (var row = 0; row < Grid.RowDefinitions.Count; row++)
        {
            var element = Grid.Children
                .FirstOrDefault(child => Grid.GetRow(child) == row && Grid.GetColumn(child) == lastColumnIndex);

            var updatedCells = _exprGrid.ClearCell(new CellPointer(lastColumnIndex - 1, row));
            RecUpdateViewFromList(updatedCells);

            if (element != null)
            {
                Grid.Children.Remove(element);
            }
        }
    }

    private void AddRowButton_Clicked(object sender, EventArgs e)
    {
        var newRow = Grid.RowDefinitions.Count;

        // Add a new row definition
        Grid.RowDefinitions.Add(new RowDefinition());

        // Add label for the row number
        var label = NewLabel(newRow.ToString());

        MauiGrid.SetRow(label, newRow);
        MauiGrid.SetColumn(label, 0);
        Grid.Children.Add(label);

        // Add entry cells for the new row
        for (var col = 1; col < Grid.ColumnDefinitions.Count; col++)
        {
            var entry = NewEmptyEntry();

            MauiGrid.SetRow(entry, newRow);
            MauiGrid.SetColumn(entry, col);
            Grid.Children.Add(entry);
        }
    }

    private void AddColumnButton_Clicked(object sender, EventArgs e)
    {
        var newColumn = Grid.ColumnDefinitions.Count;

        // Add a new column definition
        Grid.ColumnDefinitions.Add(new ColumnDefinition());

        // Add label for the column name
        var label = NewLabel(CellPointer.NumberToColumn(newColumn));

        MauiGrid.SetRow(label, 0);
        MauiGrid.SetColumn(label, newColumn);
        Grid.Children.Add(label);

        // Add entry cells for the new column
        for (var row = 1; row < Grid.RowDefinitions.Count; row++)
        {
            var entry = NewEmptyEntry();

            MauiGrid.SetRow(entry, row);
            MauiGrid.SetColumn(entry, newColumn);
            Grid.Children.Add(entry);
        }
    }

    private Entry NewEmptyEntry()
    {
        var entry = new Entry
        {
            Text = string.Empty,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
            MinimumWidthRequest = 100

        };

        entry.Focused += Entry_Focused!;
        entry.Unfocused += Entry_Unfocused!;

        return entry;
    }

    private static Label NewLabel(string name)
    {
        return new Label
        {
            Text = name,

            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,

            MinimumWidthRequest = 50,
            MinimumHeightRequest = 40,

            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
    }

    private void RecUpdateViewFromList(List<CellPointer> pointers)
    {
        foreach (var pointer in pointers)
        {
            UpdateCellView(pointer, _exprGrid.GetCellData(pointer));
            RecUpdateViewFromList(_exprGrid.GetDependents(pointer));
        }
    }

    private void UpdateViewFromExprGrid()
    {
        Grid.IsVisible = false;

        Grid.Clear();
        Grid.Children.Clear();
        Grid.RowDefinitions.Clear();
        Grid.ColumnDefinitions.Clear();

        CreateGrid(_exprGrid.Columns(), _exprGrid.Rows());

        Grid.IsVisible = true;

        foreach (var (pointer, expression) in _exprGrid)
        {
            UpdateCellView(pointer, expression);
        }
    }

    private void UpdateCellView(CellPointer pointer, string expression)
    {
        if (Grid.Children.FirstOrDefault(child =>
                Grid.GetRow(child) == pointer.Row + 1 &&
                Grid.GetColumn(child) == pointer.Column + 1
            ) is not Entry entry) return;

        if (string.IsNullOrEmpty(expression))
        {
            entry.Text = string.Empty;
            entry.BackgroundColor = Pallete.DefaultBackground;
            entry.TextColor = Colors.White;
            return;
        }

        var value = expression;
        try
        {
            var expressionResult = _gridCalculator.EvaluateForCell(expression, pointer);

            value = expressionResult % 1 == 0
                ? ((Int128)expressionResult).ToString()
                : expressionResult.ToString(CultureInfo.InvariantCulture);

            entry.BackgroundColor = Pallete.DefaultBackground;
            entry.TextColor = Colors.White;
        }
        catch (Exception ex)
        {
            entry.BackgroundColor = Pallete.ErrorBackground;
            entry.TextColor = Pallete.ErrorText;

            if (ex is not DivideByZeroException)
            {
                var columnString = CellPointer.NumberToColumn(pointer.Column + 1);
                DisplayAlert("Error", $"${columnString}${pointer.Row + 1}: {ex.Message}", "OK");
            }
        }

        entry.Text = value;
    }
}