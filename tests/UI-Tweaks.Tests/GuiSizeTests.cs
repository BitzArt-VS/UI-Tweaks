using BitzArt.UI.Tweaks.Gui;

namespace BitzArt.UI.Tweaks.Tests;

public class GuiSizeTests
{
    [Fact]
    public void NumericConversion_IntegerValue_ShouldCreateFixedSize()
    {
        // Arrange
        const int value = 42;

        // Act
        GuiSize size = value;

        // Assert
        Assert.True(size.IsFixed);
        Assert.Equal(value, size.Resolve(100));
    }

    [Fact]
    public void StringConversion_PercentageValue_ShouldCreateFractionalSize()
    {
        // Arrange
        const string value = "50%";
        const double availableSize = 200;

        // Act
        GuiSize size = value;
        double resolvedSize = size.Resolve(availableSize);

        // Assert
        Assert.True(size.IsFraction);
        Assert.Equal(100, resolvedSize);
    }

    [Fact]
    public void StringConversion_NullableTarget_ShouldBindPercentageValue()
    {
        // Arrange
        const string value = "50%";
        const double availableSize = 200;

        // Act
        GuiSize? size = AcceptSize(value);
        double resolvedSize = size.GetValueOrDefault().Resolve(availableSize);

        // Assert
        Assert.NotNull(size);
        Assert.Equal(100, resolvedSize);
    }

    [Fact]
    public void Resolve_FractionWithBounds_ShouldClampSize()
    {
        // Arrange
        GuiSize size = GuiSize.Fraction(0.25, minimum: 150, maximum: 300);

        // Act
        double minimumResult = size.Resolve(400);
        double proportionalResult = size.Resolve(1000);
        double maximumResult = size.Resolve(2000);

        // Assert
        Assert.Equal(150, minimumResult);
        Assert.Equal(250, proportionalResult);
        Assert.Equal(300, maximumResult);
    }

    private static GuiSize? AcceptSize(GuiSize? size) => size;
}
