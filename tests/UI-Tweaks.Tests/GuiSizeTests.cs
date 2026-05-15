using BitzArt.UI.Tweaks.Gui;

namespace BitzArt.UI.Tweaks.Tests;

public class GuiSizeTests
{
    [Fact]
    public void NumericConversionCreatesFixedSize()
    {
        GuiSize size = 42;

        Assert.True(size.IsFixed);
        Assert.Equal(42, size.Resolve(100));
    }

    [Fact]
    public void PercentageStringCreatesFractionalSize()
    {
        GuiSize size = "50%";

        Assert.True(size.IsFraction);
        Assert.Equal(100, size.Resolve(200));
    }

    [Fact]
    public void PercentageStringCanBindToNullableSizeParameter()
    {
        GuiSize? size = AcceptSize("50%");

        Assert.NotNull(size);
        Assert.Equal(100, size.Value.Resolve(200));
    }

    [Fact]
    public void FractionClampsResolvedSize()
    {
        GuiSize size = GuiSize.Fraction(0.25, minimum: 150, maximum: 300);

        Assert.Equal(150, size.Resolve(400));
        Assert.Equal(250, size.Resolve(1000));
        Assert.Equal(300, size.Resolve(2000));
    }

    private static GuiSize? AcceptSize(GuiSize? size) => size;
}
