using BitzArt.UI.Tweaks.Gui;

namespace BitzArt.UI.Tweaks.Tests;

public class GuiComponentLayoutParametersTests
{
    [Fact]
    public void ResolveBounds_AvailablePosition_ShouldPreservePosition()
    {
        // Arrange
        var layoutParameters = new GuiComponentLayoutParameters();
        var availablePosition = new GuiPoint(10, 20, IsAbsolute: true);
        var availableBounds = new GuiBounds(
            availablePosition,
            new GuiSize(null, null));

        // Act
        GuiBounds result =
            layoutParameters.ResolveBounds(availableBounds);

        // Assert
        Assert.Equal(availablePosition, result.Position);
    }

    [Fact]
    public void ResolveBounds_AvailableSize_ShouldUseAvailableSizeProvisionally()
    {
        // Arrange
        var layoutParameters = new GuiComponentLayoutParameters();
        var availableSize = new GuiSize(300, 200);
        var availableBounds = new GuiBounds(
            null,
            availableSize);

        // Act
        GuiBounds result =
            layoutParameters.ResolveBounds(availableBounds);

        // Assert
        Assert.Equal(availableSize, result.Size);
    }
}
