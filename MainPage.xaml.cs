using System.Globalization;
using Lab1.Services.FileSavePicker;
using MauiGrid = Microsoft.Maui.Controls.Grid;
using SGrid = Lab1.Grid.Grid;

namespace Lab1;

public partial class MainPage : ContentPage
{
    private const string DefaultGridSaveFileName = "gridsave.json";

    private const int MinColumnsNumber = 22;
    private const int MinRowsNumber = 22;

    private readonly SGrid _expressionsGrid;
    private readonly GridCalculator.GridCalculator _gridCalculator;

    public MainPage()
    {
        InitializeComponent();
        CreateGrid();

        _expressionsGrid = new SGrid(MinRowsNumber, MinColumnsNumber);
        _gridCalculator = new GridCalculator.GridCalculator(_expressionsGrid);
    }

    private void CreateGrid()
    {
        AddColumnsAndColumnLabels();
        AddRowsAndCellEntries();
    }

    private void AddColumnsAndColumnLabels()
    {
        for (var col = 0; col < MinColumnsNumber; col++)
        {
            Grid.ColumnDefinitions.Add(new ColumnDefinition());

            if (col <= 0)
            {
                continue;
            }

            var label = NewLabel(GetColumnName(col));

            // Double click recognition for row width adjustment.
            var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            var column = col;
            tapGesture.Tapped += (_, _) => AdjustColumnWidth(column + 1);
            label.GestureRecognizers.Add(tapGesture);

            MauiGrid.SetRow(label, 0);
            MauiGrid.SetColumn(label, col);
            Grid.Children.Add(label);
        }
    }

    private void AddRowsAndCellEntries()
    {
        for (var row = 0; row < MinRowsNumber; row++)
        {
            Grid.RowDefinitions.Add(new RowDefinition());

            var label = NewLabel((row + 1).ToString());

            MauiGrid.SetRow(label, row + 1);
            MauiGrid.SetColumn(label, 0);
            Grid.Children.Add(label);

            for (int col = 0; col < MinColumnsNumber; col++)
            {
                var entry = NewEmptyEntry();

                MauiGrid.SetRow(entry, row + 1);
                MauiGrid.SetColumn(entry, col + 1);
                Grid.Children.Add(entry);
            }
        }
    }

    private static string GetColumnName(int colIndex)
    {
        var dividend = colIndex;
        var columnName = string.Empty;

        while (dividend > 0)
        {
            int modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar(65 + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName;
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

        if (columnIndex < Grid.ColumnDefinitions.Count)
        {
            Grid.ColumnDefinitions[columnIndex].Width = new GridLength(maxWidth, GridUnitType.Absolute);
        }

        // Trigger a layout update to immediately apply the new widths.
        Grid.Dispatcher.Dispatch(() => {
            Grid.IsVisible = false;
            Grid.IsVisible = true;
        });
    }

    private void Entry_Unfocused(object sender, FocusEventArgs e)
    {
        var entry = (Entry)sender;

        var column = MauiGrid.GetColumn(entry);
        var row = MauiGrid.GetRow(entry);

        if (string.IsNullOrEmpty(entry.Text))
        {
            _expressionsGrid[row, column] = null;
            return;
        }

        try
        {
            var result = _gridCalculator.Evaluate(entry.Text);

            _expressionsGrid[row, column] = entry.Text;

            if (result % 1 == 0)
            {
                entry.Text = ((Int128)result).ToString();
                return;
            }

            entry.Text = result.ToString(CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void Entry_Focused(object sender, FocusEventArgs e)
    {
        var entry = (Entry)sender;

        var column = MauiGrid.GetColumn(entry);
        var row = MauiGrid.GetRow(entry);

        var rawExpression = _expressionsGrid[row, column];
        if (string.IsNullOrEmpty(rawExpression))
        {
            return;
        }

        entry.Text = rawExpression;
    }

    private void CalculateButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var result = _gridCalculator.Evaluate(TextInput.Text);
            Console.WriteLine(result);
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void SaveButton_Clicked(object sender, EventArgs e)
    {
        var filePath = await FileSavePicker.PickAsync(DefaultGridSaveFileName);
        if (string.IsNullOrEmpty(filePath)) return;

        var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
        _expressionsGrid.WriteToJsonStreamAsync(fileStream);

        fileStream.Close();
    }

    private async void ReadButton_Clicked(object sender, EventArgs e)
    {
        var fileResult = await FilePicker.PickAsync();

        if (fileResult == null) return;
        if (string.IsNullOrEmpty(fileResult.FullPath)) return;

        var fileStream = await fileResult.OpenReadAsync();
        await _expressionsGrid.ReadFromJsonStreamAsync(fileStream);

        fileStream.Close();

        UpdateViewFromInnerGrid();
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
        if (Grid.RowDefinitions.Count <= MinRowsNumber)
        {
            return;
        }

        int lastRowIndex = Grid.RowDefinitions.Count - 1;
        Grid.RowDefinitions.RemoveAt(lastRowIndex);
        for (int col = 0; col < Grid.ColumnDefinitions.Count; col++)
        {
            var element = Grid.Children
                .FirstOrDefault(child => Grid.GetRow(child) == lastRowIndex && Grid.GetColumn(child) == col);

            if (element != null)
            {
                Grid.Children.Remove(element);
            }
        }
    }

    private void DeleteColumnButton_Clicked(object sender, EventArgs e)
    {
        if (Grid.ColumnDefinitions.Count <= MinColumnsNumber)
        {
            return;
        }

        int lastColumnIndex = Grid.ColumnDefinitions.Count - 1;

        Grid.ColumnDefinitions.RemoveAt(lastColumnIndex);

        for (int row = 0; row < Grid.RowDefinitions.Count; row++)
        {
            var element = Grid.Children
                .FirstOrDefault(child => Grid.GetRow(child) == row && Grid.GetColumn(child) == lastColumnIndex);

            if (element != null)
            {
                Grid.Children.Remove(element);
            }
        }
    }

    private void AddRowButton_Clicked(object sender, EventArgs e)
    {
        int newRow = Grid.RowDefinitions.Count;

        // Add a new row definition
        Grid.RowDefinitions.Add(new RowDefinition());

        // Add label for the row number
        var label = NewLabel(newRow.ToString());

        MauiGrid.SetRow(label, newRow);
        MauiGrid.SetColumn(label, 0);
        Grid.Children.Add(label);

        // Add entry cells for the new row
        for (var col = 0; col < MinColumnsNumber; col++)
        {
            var entry = NewEmptyEntry();

            MauiGrid.SetRow(entry, newRow);
            MauiGrid.SetColumn(entry, col + 1);
            Grid.Children.Add(entry);
        }
    }

    private void AddColumnButton_Clicked(object sender, EventArgs e)
    {
        int newColumn = Grid.ColumnDefinitions.Count;

        // Add a new column definition
        Grid.ColumnDefinitions.Add(new ColumnDefinition());

        // Add label for the column name
        var label = NewLabel(GetColumnName(newColumn));

        MauiGrid.SetRow(label, 0);
        MauiGrid.SetColumn(label, newColumn);
        Grid.Children.Add(label);

        // Add entry cells for the new column
        for (int row = 0; row < MinRowsNumber; row++)
        {
            var entry = NewEmptyEntry();

            MauiGrid.SetRow(entry, row + 1);
            MauiGrid.SetColumn(entry, newColumn);
            Grid.Children.Add(entry);
        }
    }

    private Entry NewEmptyEntry()
    {
        var entry = new Entry
        {
            Text = "",
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

    private void UpdateViewFromInnerGrid()
    {
        foreach (var (row, column, expression) in _expressionsGrid)
        {
            if (string.IsNullOrEmpty(expression))
            {
                continue;
            }

            if (Grid.Children.FirstOrDefault(child =>
                    Grid.GetRow(child) == row && Grid.GetColumn(child) == column
                ) is not Entry entry) continue;

            var value = expression;
            try
            {
                var expressionResult = _gridCalculator.Evaluate(expression);

                value = expressionResult % 1 == 0 ?
                    ((Int128)expressionResult).ToString() :
                    expressionResult.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                var columnString = NumberToColumn(column);
                DisplayAlert("Error", $"${columnString}${row}: {ex.Message}", "OK");
            }

            entry.Text = value;
        }
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