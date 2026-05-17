using BitzArt.UI.Tweaks.GameStatus;
using System.Globalization;

namespace BitzArt.UI.Tweaks.Tests;

public class WorldDateTimeTests
{
    [Fact]
    public void FormatsYearZeroDateWithoutDateTime()
    {
        var worldDateTime = new WorldDateTime(0, 1, 3, 5, 7);

        var formatted = worldDateTime.ToString("d MMMM, Year y", CultureInfo.InvariantCulture);

        Assert.Equal("3 January, Year 0", formatted);
    }

    [Fact]
    public void FormatsYearZeroTime()
    {
        var worldDateTime = new WorldDateTime(0, 1, 3, 5, 7);

        var formatted = worldDateTime.ToString("HH:mm", CultureInfo.InvariantCulture);

        Assert.Equal("05:07", formatted);
    }

    [Fact]
    public void FormatsYearZeroWithPaddedYear()
    {
        var worldDateTime = new WorldDateTime(0, 1, 3, 5, 7);

        var formatted = worldDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

        Assert.Equal("0000-01-03 05:07", formatted);
    }

    [Fact]
    public void UsesDateTimeFormattingForRepresentableDates()
    {
        var worldDateTime = new WorldDateTime(1, 1, 3, 5, 7);
        var expected = new DateTime(1, 1, 3, 5, 7, 0).ToString("d MMMM, Year y", CultureInfo.InvariantCulture);

        var formatted = worldDateTime.ToString("d MMMM, Year y", CultureInfo.InvariantCulture);

        Assert.Equal(expected, formatted);
    }
}
