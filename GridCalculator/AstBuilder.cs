using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Lab1.GridCalculator.AST;
using Lab1.GridCalculator.Parser;
using Lab1.GridCalculator.AST.Terms;
using Lab1.GridCalculator.AST.Expressions;
using Expression = Lab1.GridCalculator.AST.Expression;

namespace Lab1.GridCalculator;

public class AstBuilder : CalcBaseVisitor<AstNode>
{
    public override Expression VisitExpression(CalcParser.ExpressionContext context)
    {
        return VisitAdditionExpression(context.additionExpression());
    }

    public override Expression VisitAdditionExpression(CalcParser.AdditionExpressionContext context)
    {
        return VisitBinaryExpression(context.multiplicationExpression(), context.children);
    }

    public override Expression VisitMultiplicationExpression(CalcParser.MultiplicationExpressionContext context)
    {
        return VisitBinaryExpression(context.unaryExpression(), context.children);
    }

    public override Expression VisitUnaryExpression(CalcParser.UnaryExpressionContext context)
    {
        var operation = "";
        if (context.op != null)
        {
            operation = context.op.Text;
        }
        var term = VisitTerm(context.term());

        return new UnaryOp(term, operation);
    }

    public override Expression VisitTerm(CalcParser.TermContext context)
    {
        if (context.expression() != null)
        {
            return VisitExpression(context.expression());
        }

        if (context.NUMBER() != null)
        {
            return new Number(double.Parse(context.NUMBER().GetText()));
        }

        if (context.CellPointer() != null)
        {
            return new CellPointer(context.CellPointer().GetText());
        }

        throw new NotSupportedException($"Term type not supported: {context.GetText()}");
    }

    private Expression VisitBinaryExpression<T>(IEnumerable<T> expressionsContext, IList<IParseTree> children)
        where T : ParserRuleContext
    {
        var expressions = expressionsContext.Select(e => e.Accept(this) as Expression).ToList();

        var operators = children
            .Where(c => c is ITerminalNode)
            .Select(c => c.GetText())
            .ToList();

        var leftOrPrimary = expressions[0];
        if (leftOrPrimary == null)
        {
            throw new NullReferenceException("Left side of binary expression cannot be null");
        }

        if (expressions.Count == 1)
        {
            return leftOrPrimary;
        }

        for (var i = 1; i < expressions.Count; i++)
        {
            if (expressions[i] == null)
            {
                throw new NullReferenceException("Right side of binary expression cannot be null");
            }

            leftOrPrimary = new BinaryOp(leftOrPrimary, expressions[i], operators[i - 1]);
        }

        return leftOrPrimary;
    }
}