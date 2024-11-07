using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Lab1.Grid;
using Lab1.GridCalculator.AST;
using Lab1.GridCalculator.AST.Expressions;
using Lab1.GridCalculator.AST.Terms;
using Lab1.GridCalculator.Parser;
using CellPointer = Lab1.Grid.CellPointer;
using CellPointerTerm = Lab1.GridCalculator.AST.Terms.CellPointer;

namespace Lab1.GridCalculator;

public class GridCalculator(IGrid grid)
{
    public double Evaluate(string input)
    {
        if (string.IsNullOrEmpty(input)) return 0;

        var rawExpr = ParseExpression(input);

        AstNode ast;
        try {
            ast = rawExpr.Accept(new AstBuilder());
        } catch (ParseCanceledException e) {
            throw new InvalidOperationException(e.Message);
        };

        if (ast is not Expression expr)
        {
            throw new NotSupportedException("Invalid expression");
        }

        return (double)EvaluateExpression(expr);
    }

    public double EvaluateForCell(string input, CellPointer selfPointer)
    {
        if (input.Contains(selfPointer.ToString()))
        {
            throw new InvalidOperationException("Self-reference detected");
        }

        return Evaluate(input);
    }

    private static CalcParser.ExpressionContext ParseExpression(string input)
    {
        var inputStream = new AntlrInputStream(input);
        var lexer = new CalcLexer(inputStream);

        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(ThrowingErrorListener.Instance);

        var commonTokenStream = new CommonTokenStream(lexer);

        var parser = new CalcParser(commonTokenStream);

        parser.RemoveErrorListeners();
        parser.AddErrorListener(ThrowingErrorListener.Instance);

        parser.BuildParseTree = true;
        parser.ErrorHandler = new BailErrorStrategy();

        return parser.expression();
    }

    private object EvaluateExpression(Expression expression)
    {
        switch (expression)
        {
            case Number number:
                return number.Value;

            case UnaryOp unaryOp:
                return EvaluateUnaryOp(unaryOp);

            case CellPointerTerm cellPointer:
                return EvaluateCellPointer(cellPointer);

            case BinaryOp binaryOp:
                return EvaluateBinaryOp(binaryOp);
        }

        throw new NotSupportedException(expression.GetType().ToString());
    }

    private object EvaluateCellPointer(CellPointerTerm cellPointer)
    {
        return Evaluate(grid.GetCellData(cellPointer.Pointer));
    }

    private object EvaluateUnaryOp(UnaryOp unaryOp)
    {
        var operand = EvaluateExpression(unaryOp.Operand);

        return unaryOp.Operator switch
        {
            "" => operand,
            "-" => -(double)operand,
            "+" => +(double)operand,
            _ => throw new NotSupportedException(unaryOp.Operator)
        };
    }

    private object EvaluateBinaryOp(BinaryOp binaryOp)
    {
        var left = EvaluateExpression(binaryOp.Left);

        if (binaryOp.Right == null)
        {
            return left;
        }

        var right = EvaluateExpression(binaryOp.Right);
        if (binaryOp.Operator is "/" or "%" && (double)right == 0)
            throw new DivideByZeroException();

        if(left is double r && right is double l)
            return binaryOp.Operator switch
            {
                "+" => r + l,
                "-" => r - l,
                "*" => r * l,
                "/" => r / l,
                "%" => r % l,
                _ => throw new NotSupportedException(binaryOp.Operator)
            };

        throw new NotSupportedException($"Operator {binaryOp.Operator} is not supported " +
                                        $"for types {left.GetType()} and {right.GetType()}");
    }
}

public class ThrowingErrorListener : BaseErrorListener, IAntlrErrorListener<int>
{
    public static readonly ThrowingErrorListener Instance = new();

    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine,
        string msg, RecognitionException e)
    {
        throw new ParseCanceledException($"Invalid input at line {line}:{charPositionInLine} - {msg}");
    }
}