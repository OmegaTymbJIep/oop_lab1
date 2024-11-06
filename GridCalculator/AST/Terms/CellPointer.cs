namespace Lab1.GridCalculator.AST.Terms;

public class CellPointer(string rawValue) : Term
{
    public Grid.CellPointer Pointer { get; } = new(rawValue);
}