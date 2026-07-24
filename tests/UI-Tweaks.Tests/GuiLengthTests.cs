using BitzArt.UI.Tweaks.Gui;

namespace BitzArt.UI.Tweaks.Tests;

public class GuiLengthTests
{
    [Theory]
    [InlineData(0d, 0d)]
    [InlineData(1d, 1d)]
    [InlineData(1d, 2d)]
    [InlineData(2d, 1d)]
    [InlineData(-1d, 1d)]
    [InlineData(1d, -1d)]
    [InlineData(-1d, -1d)]
    [InlineData(double.MaxValue, double.MaxValue)]
    [InlineData(double.MaxValue, 1d)]
    [InlineData(1d, double.MaxValue)]
    [InlineData(double.MinValue, double.MaxValue)]
    [InlineData(double.MaxValue, double.MinValue)]
    [InlineData(double.MinValue, double.MinValue)]
    [InlineData(1d, null)]
    public void Resolve_FixedLength_ShouldReturnFixedValue(
        double fixedValue,
        double? availableLength)
    {
        // Arrange
        GuiLength length = GuiLength.Fixed(fixedValue);

        // Act
        double? result = length.Resolve(availableLength);

        // Assert
        Assert.Equal(fixedValue, result);
    }

    [Theory]
    [InlineData(0d, 0d, 0d)]
    [InlineData(0d, 1d, 0d)]
    [InlineData(1d, 0d, 0d)]
    [InlineData(1d, 1d, 1d)]
    [InlineData(0.5d, 2d, 1d)]
    [InlineData(2d, 0.5d, 1d)]
    [InlineData(0.5d, -2d, -1d)]
    [InlineData(double.MaxValue, 0d, 0d)]
    [InlineData(1d, double.MaxValue, double.MaxValue)]
    public void Resolve_FractionalLength_ShouldScaleAvailableLength(
        double fractionalValue,
        double availableLength,
        double expected)
    {
        // Arrange
        GuiLength length = GuiLength.Fraction(fractionalValue);

        // Act
        double? result = length.Resolve(availableLength);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(-double.Epsilon)]
    [InlineData(-1d)]
    [InlineData(double.MinValue)]
    [InlineData(double.NegativeInfinity)]
    public void Fraction_NegativeValue_ShouldThrowArgumentOutOfRangeException(
        double fractionalValue)
    {
        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => GuiLength.Fraction(fractionalValue));
    }

    [Theory]
    [InlineData("0", 0d)]
    [InlineData("1", 1d)]
    [InlineData("-1", -1d)]
    [InlineData("1.5", 1.5d)]
    [InlineData(" 1 ", 1d)]
    public void Parse_FixedValue_ShouldResolveExpectedLength(
        string value,
        double expected)
    {
        // Act
        GuiLength length = GuiLength.Parse(value);
        double? result = length.Resolve(null);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("0%", 1d, 0d)]
    [InlineData("50%", 2d, 1d)]
    [InlineData("100%", 2d, 2d)]
    [InlineData("200%", 0.5d, 1d)]
    [InlineData(" 50 % ", 2d, 1d)]
    [InlineData("12.5%", 8d, 1d)]
    public void Parse_FractionalValue_ShouldResolveExpectedLength(
        string value,
        double availableLength,
        double expected)
    {
        // Act
        GuiLength length = GuiLength.Parse(value);
        double? result = length.Resolve(availableLength);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    [InlineData("not-a-length")]
    [InlineData("%")]
    [InlineData("not-a-percentage%")]
    [InlineData("-1%")]
    public void Parse_InvalidValue_ShouldThrowException(string? value)
    {
        // Act + Assert
        Assert.ThrowsAny<Exception>(
            () => GuiLength.Parse(value!));
    }
}
