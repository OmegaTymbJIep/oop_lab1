namespace Lab1.GridCalculator.AST.Expressions;

public class UnaryOp(Expression operand, string op) : Expression
{
    public Expression Operand { get; protected set; } = operand;
    public string Operator { get; protected set; } = op;
}