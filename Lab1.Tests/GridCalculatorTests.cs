using System.Collections;
using Lab1.Core.Grid;
using Lab1.Core.GridCalculator;

namespace Lab1.Tests;

[TestFixture]
public class GridCalculatorTests
{
    private GridCalculator _calculator;
    private MockGrid _mockGrid;

    [SetUp]
    public void Setup()
    {
        _mockGrid = new MockGrid();
        _calculator = new GridCalculator(_mockGrid);
    }

    [Test]
    public void Evaluate_SimpleNumber_ReturnsNumber()
    {
        var result = _calculator.Evaluate("42");
        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void Evaluate_Addition_ReturnsCorrectResult()
    {
        var result = _calculator.Evaluate("2 + 3");
        Assert.That(actual: result, Is.EqualTo(5));
    }

    [Test]
    public void Evaluate_Subtraction_ReturnsCorrectResult()
    {
        var result = _calculator.Evaluate("10 - 7");
        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public void Evaluate_Multiplication_ReturnsCorrectResult()
    {
        var result = _calculator.Evaluate("4 * 5");
        Assert.That(result, Is.EqualTo(20));
    }

    [Test]
    public void Evaluate_Division_ReturnsCorrectResult()
    {
        var result = _calculator.Evaluate("20 / 4");
        Assert.That(result, Is.EqualTo(5));
    }

    [Test]
    public void Evaluate_Modulo_ReturnsCorrectResult()
    {
        var result = _calculator.Evaluate("10 % 3");
        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void Evaluate_Exponentiation_ReturnsCorrectResult()
    {
        var result = _calculator.Evaluate("2 ** 3");
        Assert.That(result, Is.EqualTo(8));
    }

    [Test]
    public void Evaluate_UnaryMinus_ReturnsCorrectResult()
    {
        var result = _calculator.Evaluate("-5");
        Assert.That(result, Is.EqualTo(-5));
    }

    [Test]
    public void Evaluate_UnaryPlus_ReturnsCorrectResult()
    {
        var result = _calculator.Evaluate("+5");
        Assert.That(result, Is.EqualTo(5));
    }

    [Test]
    public void Evaluate_IncFunction_ReturnsCorrectResult()
    {
        var result = _calculator.Evaluate("inc(5)");
        Assert.That(result, Is.EqualTo(6));
    }

    [Test]
    public void Evaluate_DecFunction_ReturnsCorrectResult()
    {
        var result = _calculator.Evaluate("dec(5)");
        Assert.That(result, Is.EqualTo(4));
    }

    [Test]
    public void Evaluate_NestedFunctions_ReturnsCorrectResult()
    {
        var result = _calculator.Evaluate("inc(dec(10))");
        Assert.That(result, Is.EqualTo(10));
    }

    [Test]
    public void Evaluate_FunctionWithExpression_ReturnsCorrectResult()
    {
        var result = _calculator.Evaluate("inc(2 + 3)");
        Assert.That(result, Is.EqualTo(6));
    }

    [Test]
    public void Evaluate_ExpressionWithFunctions_ReturnsCorrectResult()
    {
        var result = _calculator.Evaluate("inc(2) + dec(4)");
        Assert.That(result, Is.EqualTo(6));
    }

    [Test]
    public void Evaluate_CellReference_ReturnsCellValue()
    {
        _mockGrid.SetCellData(new CellPointer("$A$1"), "10");
        var result = _calculator.Evaluate("$A$1");
        Assert.That(result, Is.EqualTo(10));
    }

    [Test]
    public void Evaluate_FunctionWithCellReference_ReturnsCorrectResult()
    {
        _mockGrid.SetCellData(new CellPointer("$A$1"), "5");
        var result = _calculator.Evaluate("inc($A$1)");
        Assert.That(result, Is.EqualTo(6));
    }

    [Test]
    public void Evaluate_ComplexExpression_ReturnsCorrectResult()
    {
        _mockGrid.SetCellData(new CellPointer("$B$2"), "3");
        var result = _calculator.Evaluate("inc(2 * $B$2 + dec(4)) ** 2");
        Assert.That(result, Is.EqualTo(100)); // (inc(2 * 3 + dec(4))) ** 2 = (inc(6 + 3)) ** 2 = (inc(9)) ** 2 = 10 ** 2 = 100
    }

    [Test]
    public void Evaluate_InvalidFunction_ThrowsException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _calculator.Evaluate("unknownFunc(5)"));
    }

    [Test]
    public void Evaluate_DivideByZero_ThrowsException()
    {
        Assert.Throws<DivideByZeroException>(() =>
            _calculator.Evaluate("10 / 0"));
    }

    [Test]
    public void Evaluate_SelfReference_ThrowsException()
    {
        var selfPointer = new CellPointer("$A$1");
        Assert.Throws<InvalidOperationException>(() =>
            _calculator.EvaluateForCell("$A$1 + 5", selfPointer));
    }
}

// Mock implementation of IGrid for testing purposes
public class MockGrid : IGrid
{
    private readonly Dictionary<CellPointer, string> _cellData = new();

    public string GetCellData(CellPointer pointer)
    {
        return _cellData.TryGetValue(pointer, out var data) ? data : string.Empty;
    }

    public void SetCellData(CellPointer pointer, string data)
    {
        _cellData[pointer] = data;
    }

    public int Rows()
    {
        return 10;
    }

    public int Columns()
    {
        return 10;
    }

    public Task WriteToJsonStreamAsync(Stream stream)
    {
        throw new NotImplementedException();
    }

    public Task ReadFromJsonStreamAsync(Stream stream)
    {
        throw new NotImplementedException();
    }

    public List<CellPointer> UpdateCell(CellPointer pointer, string value)
    {
        SetCellData(pointer, value);
        return [];
    }

    public List<CellPointer> ClearCell(CellPointer pointer)
    {
        _cellData.Remove(pointer);
        return [];
    }

    public List<CellPointer> GetDependents(CellPointer pointer)
    {
        return [];
    }

    public IEnumerator<(CellPointer pointer, string Value)> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}