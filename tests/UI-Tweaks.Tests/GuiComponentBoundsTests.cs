using BitzArt.UI.Tweaks.Gui;

namespace BitzArt.UI.Tweaks.Tests;

public class GuiComponentBoundsTests
{
    [Fact]
    public void Deflate_Thickness_ShouldMoveBoundsInward()
    {
        // Arrange
        var bounds = new GuiComponentBounds(
            new GuiPoint(10, 20),
            new GuiSize(100, 80));
        var thickness = new GuiThickness(
            Top: 5,
            Right: 10,
            Bottom: 15,
            Left: 20);
        var expected = new GuiComponentBounds(
            new GuiPoint(30, 25),
            new GuiSize(70, 60));

        // Act
        GuiComponentBounds result =
            bounds.Deflate(thickness);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Inflate_Thickness_ShouldMoveBoundsOutward()
    {
        // Arrange
        var bounds = new GuiComponentBounds(
            new GuiPoint(30, 25),
            new GuiSize(70, 60));
        var thickness = new GuiThickness(
            Top: 5,
            Right: 10,
            Bottom: 15,
            Left: 20);
        var expected = new GuiComponentBounds(
            new GuiPoint(10, 20),
            new GuiSize(100, 80));

        // Act
        GuiComponentBounds result =
            bounds.Inflate(thickness);

        // Assert
        Assert.Equal(expected, result);
    }
}
