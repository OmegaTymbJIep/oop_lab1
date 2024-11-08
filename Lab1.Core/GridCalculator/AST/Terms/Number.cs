namespace Lab1.Core.GridCalculator.AST.Terms;

public class Number(double value) : Term
{
    public double Value { get; protected set; } = value;
}