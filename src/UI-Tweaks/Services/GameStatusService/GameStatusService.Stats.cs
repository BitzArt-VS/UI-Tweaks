using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace BitzArt.UI.Tweaks.Services;

public partial class GameStatusService
{
    private DetailRecord<float> _playerHealthCurrent = null!;
    private DetailRecord<float> _playerHealthMax = null!;
    private DetailRecord<float> _playerHealthPercent = null!;

    private DetailRecord<float> _playerSatietyCurrent = null!;
    private DetailRecord<float> _playerSatietyMax = null!;
    private DetailRecord<float> _playerSatietyPercent = null!;
    private DetailRecord<float> _playerSatietyHungerRate = null!;

    private DetailRecord<float> _playerTemporalStability = null!;

    private DetailRecord<PlayerCoordinates> _playerLocationCoordinates = null!;
    private DetailRecord<float> _playerLocationCoordinatesX = null!;
    private DetailRecord<float> _playerLocationCoordinatesY = null!;
    private DetailRecord<float> _playerLocationCoordinatesZ = null!;

    private DetailRecord<float> _playerLocationTemperatureCelsius = null!;
    private DetailRecord<float> _playerLocationTemperatureFahrenheit = null!;

    private DetailRecord<float> _playerLocationAverageYearlyTemperatureCelsius = null!;
    private DetailRecord<float> _playerLocationAverageYearlyTemperatureFahrenheit = null!;
    private DetailRecord<float> _playerLocationAveragePrecipitationPercent = null!;
    private DetailRecord<float> _playerLocationAverageForestationPercent = null!;
    private DetailRecord<float> _playerLocationAverageShrubberyPercent = null!;

    private DetailRecord<float> _playerLocationTemporalStability = null!;

    private DetailRecord<DateTime> _worldDateTime = null!;

    private record struct PlayerCoordinates(float X, float Y, float Z);

    private void InitStats()
    {
        _playerHealthCurrent = new(GameStatusDetailType.PlayerCurrentHealth, "player-health-current", x => (float)Math.Round(x, 1));
        _detailRecords.Add(_playerHealthCurrent);
        _playerHealthMax = new(GameStatusDetailType.PlayerMaxHealth, "player-health-max", x => (float)Math.Round(x, 1));
        _detailRecords.Add(_playerHealthMax);
        _playerHealthPercent = new(GameStatusDetailType.PlayerHealthPercentage, "player-health-percent", x => (float)Math.Round(x, 0));
        _detailRecords.Add(_playerHealthPercent);

        _playerSatietyCurrent = new(GameStatusDetailType.PlayerCurrentSatiety, "player-satiety-current", x => (float)Math.Round(x, 0));
        _detailRecords.Add(_playerSatietyCurrent);
        _playerSatietyMax = new(GameStatusDetailType.PlayerMaxSatiety, "player-satiety-max", x => (float)Math.Round(x, 0));
        _detailRecords.Add(_playerSatietyMax);
        _playerSatietyPercent = new(GameStatusDetailType.PlayerSatietyPercentage, "player-satiety-percent", x => (float)Math.Round(x, 0));
        _detailRecords.Add(_playerSatietyPercent);
        _playerSatietyHungerRate = new(GameStatusDetailType.PlayerSatietyHungerRate, "player-satiety-hunger", x => (float)Math.Round(x * 100, 0));
        _detailRecords.Add(_playerSatietyHungerRate);

        _playerTemporalStability = new(GameStatusDetailType.PlayerTemporalStability, "player-temporal-stability", x => (float)Math.Round(x * 100, 0));
        _detailRecords.Add(_playerTemporalStability);

        _playerLocationCoordinates = new(GameStatusDetailType.PlayerLocationCoordinates, "player-location-coordinates", (coords) => new((float)Math.Round(coords.X, 0), (float)Math.Round(coords.Y, 0), (float)Math.Round(coords.Z, 0)));
        _detailRecords.Add(_playerLocationCoordinates);
        _playerLocationCoordinatesX = new(GameStatusDetailType.PlayerLocationCoordinatesX, "player-location-coordinates-x", x => (float)Math.Round(x, 0));
        _detailRecords.Add(_playerLocationCoordinatesX);
        _playerLocationCoordinatesY = new(GameStatusDetailType.PlayerLocationCoordinatesY, "player-location-coordinates-y", x => (float)Math.Round(x, 0));
        _detailRecords.Add(_playerLocationCoordinatesY);
        _playerLocationCoordinatesZ = new(GameStatusDetailType.PlayerLocationCoordinatesZ, "player-location-coordinates-z", x => (float)Math.Round(x, 0));
        _detailRecords.Add(_playerLocationCoordinatesZ);

        _playerLocationTemperatureCelsius = new(GameStatusDetailType.PlayerLocationTemperatureCelsius, "player-location-temperature-celsius", x => (float)Math.Round(x, 0));
        _detailRecords.Add(_playerLocationTemperatureCelsius);
        _playerLocationTemperatureFahrenheit = new(GameStatusDetailType.PlayerLocationTemperatureFahrenheit, "player-location-temperature-fahrenheit", x => (float)Math.Round(x, 0));
        _detailRecords.Add(_playerLocationTemperatureFahrenheit);

        _playerLocationAverageYearlyTemperatureCelsius = new(GameStatusDetailType.PlayerLocationAverageYearlyTemperatureCelsius, "player-location-average-yearly-temperature-celsius", x => (float)Math.Round(x, 0));
        _detailRecords.Add(_playerLocationAverageYearlyTemperatureCelsius);
        _playerLocationAverageYearlyTemperatureFahrenheit = new(GameStatusDetailType.PlayerLocationAverageYearlyTemperatureFahrenheit, "player-location-average-yearly-temperature-fahrenheit", x => (float)Math.Round(x, 0));
        _detailRecords.Add(_playerLocationAverageYearlyTemperatureFahrenheit);

        _playerLocationAveragePrecipitationPercent = new(GameStatusDetailType.PlayerLocationAveragePrecipitation, "player-location-average-precipitation-precent", x => (float)Math.Round(x, 1));
        _detailRecords.Add(_playerLocationAveragePrecipitationPercent);

        _playerLocationAverageForestationPercent = new(GameStatusDetailType.PlayerLocationAverageForestation, "player-location-average-forestation-percent", x => (float)Math.Round(x, 1));
        _detailRecords.Add(_playerLocationAverageForestationPercent);
        _playerLocationAverageShrubberyPercent = new(GameStatusDetailType.PlayerLocationAverageShrubbery, "player-location-average-shrubbery-percent", x => (float)Math.Round(x, 1));
        _detailRecords.Add(_playerLocationAverageShrubberyPercent);

        _playerLocationTemporalStability = new(GameStatusDetailType.PlayerLocationTemporalStability, "player-location-temporal-stability", x => (float)Math.Round(x * 100, 0));
        _detailRecords.Add(_playerLocationTemporalStability);

        _worldDateTime = new(GameStatusDetailType.WorldDateTime, "world-date-time");
        _detailRecords.Add(_worldDateTime);
    }

    private void UpdateStats()
    {
        if (_playerEntity?.WatchedAttributes is null)
        {
            throw new InvalidOperationException("Player entity or watched attributes not available.");
        }

        var healthAttribute = _playerEntity.WatchedAttributes.GetTreeAttribute("health");

        var healthCurrent = healthAttribute?.TryGetFloat("currenthealth");
        var healthMax = healthAttribute?.TryGetFloat("maxhealth");
        float? healthPercent = healthCurrent.HasValue && healthMax.HasValue && healthMax.Value > 0
            ? healthCurrent.Value / healthMax.Value * 100.0f
            : null;

        var hungerAttribute = _playerEntity.WatchedAttributes.GetTreeAttribute("hunger");

        var satietyCurrent = hungerAttribute?.TryGetFloat("currentsaturation");
        var satietyMax = hungerAttribute?.TryGetFloat("maxsaturation");
        float? satietyPercent = satietyCurrent.HasValue && satietyMax.HasValue && satietyMax.Value > 0
            ? satietyCurrent.Value / satietyMax.Value * 100.0f
            : null;

        var hungerRate = _playerEntity.Stats.GetBlended("hungerrate");

        var playerTemporalStability = (float)_playerEntity.WatchedAttributes.GetDouble("temporalStability");

        var absolutePos = _playerEntity.Pos.AsBlockPos;
        var spawnPos = _clientApi.World.DefaultSpawnPosition.AsBlockPos;

        int relativeX = absolutePos.X - spawnPos.X;
        int relativeZ = absolutePos.Z - spawnPos.Z;

        var playerLocationCoordinates = new PlayerCoordinates(relativeX, absolutePos.Y, relativeZ);

        var calendar = _clientApi.World.Calendar;

        var year = calendar.Year + 1;
        var month = calendar.Month;
        var dayOfMonth = calendar.DayOfYear - ((month - 1) * calendar.DaysPerMonth) + 1;

        var date = new DateOnly(year, month, dayOfMonth);
        var elapsedSeconds = (int)(calendar.ElapsedSeconds - ((long)calendar.ElapsedHours * 60 * 60));
        var time = new TimeOnly((int)calendar.HourOfDay, elapsedSeconds / 60);

        var worldDateTime = new DateTime(date, time);

        var playerLocationClimateConditions = _clientApi.World.BlockAccessor.GetClimateAt(absolutePos);

        var playerLocationTemperatureCelsius = playerLocationClimateConditions.Temperature;
        var playerLocationTemperatureFahrenheit = ToFahrenheit(playerLocationTemperatureCelsius);

        var playerLocationAverageYearlyTemperatureCelsius = playerLocationClimateConditions.WorldGenTemperature;
        var playerLocationAverageYearlyTemperatureFahrenheit = ToFahrenheit(playerLocationAverageYearlyTemperatureCelsius);

        var playerLocationAveragePrecipitationPercent = playerLocationClimateConditions.Rainfall * 100.0f;

        var playerLocationAverageForestationPercent = playerLocationClimateConditions.ForestDensity * 100.0f;
        var playerLocationAverageShrubberyPercent = playerLocationClimateConditions.ShrubDensity * 100.0f;

        _temporalStabilitySystem ??= _clientApi.ModLoader.GetModSystem<SystemTemporalStability>();
        var playerLocationTemporalStability = _temporalStabilitySystem!.GetTemporalStability(absolutePos);

        Span<DetailRecord> affectedDetails =
        [
            _playerHealthCurrent,
            _playerHealthMax,
            _playerHealthPercent,
            _playerSatietyCurrent,
            _playerSatietyMax,
            _playerSatietyPercent,
            _playerSatietyHungerRate,
            _playerTemporalStability,
            _playerLocationCoordinates,
            _playerLocationCoordinatesX,
            _playerLocationCoordinatesY,
            _playerLocationCoordinatesZ,
            _playerLocationTemperatureCelsius,
            _playerLocationTemperatureFahrenheit,
            _playerLocationAverageYearlyTemperatureCelsius,
            _playerLocationAverageYearlyTemperatureFahrenheit,
            _playerLocationAveragePrecipitationPercent,
            _playerLocationAverageForestationPercent,
            _playerLocationAverageShrubberyPercent,
            _playerLocationTemporalStability,
            _worldDateTime
        ];

        Span<object?> values =
        [
            healthCurrent,
            healthMax,
            healthPercent,
            satietyCurrent,
            satietyMax,
            satietyPercent,
            hungerRate,
            playerTemporalStability,
            playerLocationCoordinates,
            playerLocationCoordinates.X,
            playerLocationCoordinates.Y,
            playerLocationCoordinates.Z,
            playerLocationTemperatureCelsius,
            playerLocationTemperatureFahrenheit,
            playerLocationAverageYearlyTemperatureCelsius,
            playerLocationAverageYearlyTemperatureFahrenheit,
            playerLocationAveragePrecipitationPercent,
            playerLocationAverageForestationPercent,
            playerLocationAverageShrubberyPercent,
            playerLocationTemporalStability,
            worldDateTime
        ];

        List<DetailRecord> updatedDetails = new(affectedDetails.Length);
        for (int i = 0; i < affectedDetails.Length; i++)
        {
            var detail = affectedDetails[i];
            var value = values[i];

            if (value is not null && detail.Update(value))
            {
                updatedDetails.Add(detail);
            }
        }

        _subscriptions.OnUpdate(updatedDetails);
    }

    private static float ToFahrenheit(float celsius)
        => celsius * 9 / 5 + 32;
}
