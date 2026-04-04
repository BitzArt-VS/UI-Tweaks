namespace BitzArt.UI.Tweaks.Services;

public enum GameStatusDetailType
{
    // ====================================================
    //               • 1xxx - Player status •
    // ====================================================

    // --------------- 11xx - Player health ---------------

    PlayerCurrentHealth = 1101,
    PlayerMaxHealth = 1102,

    PlayerHealthPercentage = 1111,

    // --------------- 12xx - Player satiety ---------------

    PlayerCurrentSatiety = 1201,
    PlayerMaxSatiety = 1202,

    PlayerSatietyPercentage = 1211,

    PlayerSatietyHungerRate = 1221,

    // --------- 13xx - Player temporal stability ---------

    PlayerTemporalStability = 1301,

    // ====================================================
    //              • 2xxx - Player location •
    // ====================================================

    // -------------- 21xx - Player location --------------

    PlayerLocationCoordinates = 2101,

    PlayerLocationCoordinatesX = 2111,
    PlayerLocationCoordinatesY = 2112,
    PlayerLocationCoordinatesZ = 2113,

    // -------- 22xx - Player location temperature --------

    PlayerLocationTemperatureCelsius = 2201,
    PlayerLocationTemperatureFahrenheit = 2202,

    // ---- 23xx - Player location average conditions ----

    PlayerLocationAverageYearlyTemperatureCelsius = 2301,
    PlayerLocationAverageYearlyTemperatureFahrenheit = 2302,

    PlayerLocationAveragePrecipitation = 2311,

    PlayerLocationAverageForestation = 2321,
    PlayerLocationAverageShrubbery = 2322,

    // ---- 24xx - Player location temporal stability ----

    PlayerLocationTemporalStability = 2401,

    // ====================================================
    //               • 3xxx - World status •
    // ====================================================

    // ---------------- 31xx - World time ----------------

    WorldDateTime = 3101,
}
