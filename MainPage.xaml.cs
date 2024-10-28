using Microsoft.Maui.Controls.Internals;
using Grid = Microsoft.Maui.Controls.Grid;

namespace Lab1;

public partial class MainPage : ContentPage
{
    private const int CountColumn = 20;
    private const int CountRow = 50;

    public MainPage()
    {
        InitializeComponent();
        CreateGrid();
    }

    private void CreateGrid()
    {
        AddColumnsAndColumnLabels();
        AddRowsAndCellEntries();
    }

    private void AddColumnsAndColumnLabels()
    {
        for (int col = 0; col < CountColumn + 1; col++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            if (col <= 0)
            {
                continue;
            }

            var label = NewLabel(GetColumnName(col));

            // Double click recognition for row width adjustment.
            var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            tapGesture.Tapped += (_, _) => AdjustColumnWidth(col + 1);
            label.GestureRecognizers.Add(tapGesture);

            Grid.SetRow(label, 0);
            Grid.SetColumn(label, col);
            grid.Children.Add(label);
        }
    }

    private void AddRowsAndCellEntries()
    {
        for (int row = 0; row < CountRow; row++)
        {
            grid.RowDefinitions.Add(new RowDefinition());

            var label = NewLabel((row + 1).ToString());

            Grid.SetRow(label, row + 1);
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);

            for (int col = 0; col < CountColumn; col++)
            {
                var entry = NewEmptyEntry();

                entry.Unfocused += Entry_Unfocused;
                Grid.SetRow(entry, row + 1);
                Grid.SetColumn(entry, col + 1);
                grid.Children.Add(entry);
            }
        }
    }

    private string GetColumnName(int colIndex)
    {
        int dividend = colIndex;
        string columnName = string.Empty;

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
        for (int rowIndex = 1; rowIndex <= CountRow; rowIndex++)
        {
            var entry = grid.Children.FirstOrDefault(child =>
                grid.GetRow(child) == rowIndex && grid.GetColumn(child) == columnIndex
            ) as Entry;

            if (entry != null)
            {
                double entryWidth = entry.Measure(double.PositiveInfinity, entry.Height).Request.Width;
                maxWidth = Math.Max(maxWidth, entryWidth);
            }
        }

        if (columnIndex < grid.ColumnDefinitions.Count)
        {
            grid.ColumnDefinitions[columnIndex].Width = new GridLength(maxWidth, GridUnitType.Absolute);
        }

        // Trigger a layout update to immediately apply the new widths.
        Device.BeginInvokeOnMainThread(() => {
            grid.InvalidateMeasureNonVirtual(InvalidationTrigger.MeasureChanged);
        });
    }

    private void Entry_Unfocused(object sender, FocusEventArgs e)
    {
        var entry = (Entry)sender;
        var row = Grid.GetRow(entry) - 1;
        var col = Grid.GetColumn(entry) - 1;
        var content = entry.Text;
    }

    private void CalculateButton_Clicked(object sender, EventArgs e)
    {
        // TODO
    }

    private void SaveButton_Clicked(object sender, EventArgs e)
    {
        // TODO
    }

    private void ReadButton_Clicked(object sender, EventArgs e)
    {
        // TODO
    }

    private async void ExitButton_Clicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Підтвердження", "Ви дійсно хочете вийти?", "Так", "Ні");

        if (answer)
        {
            System.Environment.Exit(0);
        }
    }

    private async void HelpButton_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Довідка", "Лабораторна робота 1. Студента Левочко Антон", "ОК");
    }

    private void DeleteRowButton_Clicked(object sender, EventArgs e)
    {
        if (grid.RowDefinitions.Count <= 1)
        {
            return;
        }

        int lastRowIndex = grid.RowDefinitions.Count - 1;
        grid.RowDefinitions.RemoveAt(lastRowIndex);
        grid.Children.RemoveAt(lastRowIndex * (CountColumn + 1));
        for (int col = 0; col < CountColumn; col++)
        {
            grid.Children.RemoveAt((lastRowIndex * CountColumn) + col + 1);
        }
    }

    private void DeleteColumnButton_Clicked(object sender, EventArgs e)
    {
        if (grid.ColumnDefinitions.Count <= 1)
        {
            return;
        }

        int lastColumnIndex = grid.ColumnDefinitions.Count - 1;
        grid.ColumnDefinitions.RemoveAt(lastColumnIndex);
        grid.Children.RemoveAt(lastColumnIndex);

        for (int row = 0; row < CountRow; row++)
        {
            grid.Children.RemoveAt(row * (CountColumn + 1) + lastColumnIndex + 1);
        }
    }

    private void AddRowButton_Clicked(object sender, EventArgs e)
    {
        int newRow = grid.RowDefinitions.Count;

        // Add a new row definition
        grid.RowDefinitions.Add(new RowDefinition());

        // Add label for the row number
        var label = NewLabel(newRow.ToString());

        Grid.SetRow(label, newRow);
        Grid.SetColumn(label, 0);
        grid.Children.Add(label);

        // Add entry cells for the new row
        for (int col = 0; col < CountColumn; col++)
        {
            var entry = NewEmptyEntry();
            entry.Unfocused += Entry_Unfocused;

            Grid.SetRow(entry, newRow);
            Grid.SetColumn(entry, col + 1);
            grid.Children.Add(entry);
        }
    }

    private void AddColumnButton_Clicked(object sender, EventArgs e)
    {
        int newColumn = grid.ColumnDefinitions.Count;

        // Add a new column definition
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        // Add label for the column name
        var label = NewLabel(GetColumnName(newColumn));

        Grid.SetRow(label, 0);
        Grid.SetColumn(label, newColumn);
        grid.Children.Add(label);

        // Add entry cells for the new column
        for (int row = 0; row < CountRow; row++)
        {
            var entry = NewEmptyEntry();

            entry.Unfocused += Entry_Unfocused;
            Grid.SetRow(entry, row + 1);
            Grid.SetColumn(entry, newColumn);
            grid.Children.Add(entry);
        }
    }

    private Entry NewEmptyEntry()
    {
        return new Entry
        {
            Text = "",
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
            MinimumWidthRequest = 100
        };
    }

    private Label NewLabel(string name)
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
}