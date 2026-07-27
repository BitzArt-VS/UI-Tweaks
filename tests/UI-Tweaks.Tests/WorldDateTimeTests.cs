using BitzArt.UI.Tweaks.GameStatus;
using System.Globalization;

namespace BitzArt.UI.Tweaks.Tests;

public class WorldDateTimeTests
{
    [Fact]
    public void ToString_YearZeroDateFormat_ShouldFormatWithoutDateTime()
    {
        // Arrange
        var worldDateTime = new WorldDateTime(0, 1, 3, 5, 7);

        // Act
        var formatted = worldDateTime.ToString("d MMMM, Year y", CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("3 January, Year 0", formatted);
    }

    [Fact]
    public void ToString_YearZeroTimeFormat_ShouldFormatTime()
    {
        // Arrange
        var worldDateTime = new WorldDateTime(0, 1, 3, 5, 7);

        // Act
        var formatted = worldDateTime.ToString("HH:mm", CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("05:07", formatted);
    }

    [Fact]
    public void ToString_YearZeroPaddedFormat_ShouldPadYear()
    {
        // Arrange
        var worldDateTime = new WorldDateTime(0, 1, 3, 5, 7);

        // Act
        var formatted = worldDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("0000-01-03 05:07", formatted);
    }

    [Fact]
    public void ToString_RepresentableDate_ShouldUseDateTimeFormatting()
    {
        // Arrange
        var worldDateTime = new WorldDateTime(1, 1, 3, 5, 7);
        var expected = new DateTime(1, 1, 3, 5, 7, 0).ToString("d MMMM, Year y", CultureInfo.InvariantCulture);

        // Act
        var formatted = worldDateTime.ToString("d MMMM, Year y", CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal(expected, formatted);
    }
}
