namespace Lab1.Core.GridCalculator.AST.Terms;

public class CellPointer(string rawValue) : Term
{
    public Grid.CellPointer Pointer { get; } = new(rawValue);
}