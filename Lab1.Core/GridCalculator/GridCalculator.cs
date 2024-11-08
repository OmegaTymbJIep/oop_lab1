using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Lab1.Core.Grid;
using Lab1.Core.GridCalculator.AST;
using Lab1.Core.GridCalculator.AST.Expressions;
using Lab1.Core.GridCalculator.AST.Terms;
using Lab1.Core.GridCalculator.Parser;
using CellPointer = Lab1.Core.Grid.CellPointer;
using CellPointerTerm = Lab1.Core.GridCalculator.AST.Terms.CellPointer;

namespace Lab1.Core.GridCalculator;

public class GridCalculator(IGrid grid)
{
    public double Evaluate(string input)
    {
        if (string.IsNullOrEmpty(input)) return 0;

        var rawExpr = ParseExpression(input);

        AstNode ast;
        try
        {
            ast = rawExpr.Accept(new AstBuilder());
        }
        catch (ParseCanceledException e)
        {
            throw new InvalidOperationException(e.Message);
        }

        if (ast is not Expression expr)
        {
            throw new NotSupportedException("Invalid expression");
        }

        return (double)EvaluateExpression(expr);
    }

    public double EvaluateForCell(string input, CellPointer selfPointer)
    {
        var pointers = CellPointer.FindPointers(input);
        if (pointers.Contains(selfPointer))
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

            case FunctionCall functionCall:
                return EvaluateFunctionCall(functionCall);

            default:
                throw new NotSupportedException($"Expression type '{expression.GetType()}' is not supported");
        }
    }

    private object EvaluateCellPointer(CellPointerTerm cellPointer)
    {
        return Evaluate(grid.GetCellData(cellPointer.Pointer));
    }

    private object EvaluateUnaryOp(UnaryOp unaryOp)
    {
        var operand = (double)EvaluateExpression(unaryOp.Operand);

        return unaryOp.Operator switch
        {
            "" => operand,
            "-" => -operand,
            "+" => +operand,
            "--" => operand - 1,
            "++" => operand + 1,
            _ => throw new NotSupportedException($"Unary operator '{unaryOp.Operator}' is not supported")
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

        if (left is double l && right is double r)
            return binaryOp.Operator switch
            {
                "+" => l + r,
                "-" => l - r,
                "*" => l * r,
                "/" => l / r,
                "%" => l % r,
                "**" => Math.Pow(l, r),
                _ => throw new NotSupportedException($"Binary operator '{binaryOp.Operator}' is not supported")
            };

        throw new NotSupportedException($"Operator '{binaryOp.Operator}' is not supported " +
                                        $"for types {left.GetType()} and {right.GetType()}");
    }

    private object EvaluateFunctionCall(FunctionCall functionCall)
    {
        var argumentValue = EvaluateExpression(functionCall.Argument);
        if (argumentValue is not double arg)
        {
            throw new InvalidOperationException("Function argument must evaluate to a number");
        }

        return functionCall.Name switch
        {
            FunctionCall.Inc => arg + 1,
            FunctionCall.Dec => arg - 1,
            _ => throw new NotSupportedException($"Function '{functionCall.Name}' is not supported")
        };
    }
}

public class ThrowingErrorListener : BaseErrorListener, IAntlrErrorListener<int>
{
    public static readonly ThrowingErrorListener Instance = new();

    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine,
        string msg, RecognitionException e)
    {
        throw new InvalidOperationException($"Invalid input at line {line}:{charPositionInLine} - {msg}");
    }
}