namespace Lab1.GridCalculator.AST.Terms;

public class FunctionCall : Term
{
    public const string Inc = "inc";
    public const string Dec = "dec";

    public string Name { get; protected set; }
    public Expression Argument { get; protected set; }

    public FunctionCall(string name, Expression argument)
    {
        Name = name;
        Argument = argument;
    }
}