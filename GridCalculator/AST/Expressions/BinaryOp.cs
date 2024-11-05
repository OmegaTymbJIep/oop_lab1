namespace Lab1.GridCalculator.AST.Expressions;

public class BinaryOp(Expression left, Expression? right, string op) : Expression
{
    public Expression Left { get; protected set; } = left;
    public Expression? Right { get; protected set; } = right;
    public string Operator { get; protected set; } = op;
}