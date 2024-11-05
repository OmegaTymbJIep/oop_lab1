namespace Lab1.GridCalculator.AST.Terms;

public class Number(double value) : Term
{
    public double Value { get; protected set; } = value;
}