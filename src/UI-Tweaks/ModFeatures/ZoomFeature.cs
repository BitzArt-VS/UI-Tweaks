using BitzArt.UI.Tweaks.Config;
using HarmonyLib;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace BitzArt.UI.Tweaks;

public sealed class ZoomFeature(UiTweaksModSystem modSystem, ZoomConfig config)
    : ModSystemFeature<UiTweaksModSystem, ZoomConfig>(modSystem, config)
{
    private const int ZoomUpdateIntervalMilliseconds = 10;
    private const float MinimumFieldOfViewDegrees = 10;
    private const float DegreesToRadians = MathF.PI / 180;
    private const float MinimumMouseSensitivityReductionStrength = 0.35f;
    private const float MaximumMouseSensitivityReductionStrength = 1f;
    private const float VignetteStrengthBoost = 0f;
    private const string HarmonyId = $"{Constants.ModId}.zoom";

    private ICoreClientAPI? _clientApi;
    private ClientMain? _clientMain;
    private Harmony? _harmony;
    private ZoomOverlayRenderer? _overlayRenderer;
    private long? _zoomUpdateListenerId;
    private int? _activeZoomKeyCode;
    private bool _isKeyUpSubscribed;
    private bool _isZoomRequested;
    private float _zoomProgress;

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client && Config.Enable;

    public override void Start(ICoreClientAPI clientApi)
    {
        if (clientApi.World is not ClientMain clientMain)
        {
            throw new InvalidOperationException("Unable to retrieve ClientMain from ICoreClientAPI.");
        }

        _clientApi = clientApi;
        _clientMain = clientMain;
        _harmony = new Harmony(HarmonyId);
        _overlayRenderer = new(clientApi);

        ZoomProjectionPatch.Patch(_harmony);
        ZoomMouseSensitivityPatch.Patch(_harmony);

        clientApi.Input.AddHotKey(ModHotKeys.Zoom, _ => StartZooming());
    }

    public override void Dispose()
    {
        UnsubscribeFromKeyUp();
        StopZoomUpdates();
        _isZoomRequested = false;
        _zoomProgress = 0;

        ApplyZoomProgress();

        _overlayRenderer?.Dispose();
        _overlayRenderer = null;

        _harmony?.UnpatchAll(_harmony.Id);
        _harmony = null;

        ZoomRuntimeState.FieldOfViewFactor = 1;
        ZoomRuntimeState.MouseSensitivityFactor = 1;

        _clientApi = null;
        _clientMain = null;

        GC.SuppressFinalize(this);
    }

    private bool StartZooming()
    {
        if (_clientApi is null || _clientMain is null)
        {
            return false;
        }

        _isZoomRequested = true;
        _activeZoomKeyCode = _clientApi.Input.GetHotKeyByCode(ModHotKeys.Zoom.Code).CurrentMapping.KeyCode;
        SubscribeToKeyUp();
        StartZoomUpdates();

        return true;
    }

    private void OnKeyUp(KeyEvent keyEvent)
    {
        if (_activeZoomKeyCode != keyEvent.KeyCode)
        {
            return;
        }

        _activeZoomKeyCode = null;
        _isZoomRequested = false;
        UnsubscribeFromKeyUp();
        StartZoomUpdates();
    }

    private void SubscribeToKeyUp()
    {
        if (_clientApi is null || _isKeyUpSubscribed)
        {
            return;
        }

        _clientApi.Event.KeyUp += OnKeyUp;
        _isKeyUpSubscribed = true;
    }

    private void UnsubscribeFromKeyUp()
    {
        if (_clientApi is null || !_isKeyUpSubscribed)
        {
            return;
        }

        _clientApi.Event.KeyUp -= OnKeyUp;
        _isKeyUpSubscribed = false;
    }

    private void StartZoomUpdates()
    {
        if (_clientApi is null || _zoomUpdateListenerId is not null)
        {
            return;
        }

        _zoomUpdateListenerId = _clientApi.Event.RegisterGameTickListener(UpdateZoom, ZoomUpdateIntervalMilliseconds);
    }

    private void StopZoomUpdates()
    {
        if (_clientApi is null || _zoomUpdateListenerId is null)
        {
            return;
        }

        _clientApi.Event.UnregisterGameTickListener(_zoomUpdateListenerId.Value);
        _zoomUpdateListenerId = null;
    }

    private void UpdateZoom(float deltaTime)
    {
        float targetZoomProgress = _isZoomRequested ? 1 : 0;
        float previousZoomProgress = _zoomProgress;
        float zoomStep = GetZoomSpeed() * deltaTime;

        _zoomProgress = MoveTowards(_zoomProgress, targetZoomProgress, zoomStep);

        if (Math.Abs(_zoomProgress - previousZoomProgress) > float.Epsilon)
        {
            ApplyZoomProgress();
        }

        if (Math.Abs(_zoomProgress - targetZoomProgress) <= float.Epsilon)
        {
            StopZoomUpdates();
        }
    }

    private void ApplyZoomProgress()
    {
        if (_clientApi is null || _clientMain is null)
        {
            return;
        }

        float baseFieldOfViewDegrees = Math.Max(MinimumFieldOfViewDegrees, _clientApi.Settings.Int["fieldOfView"]);
        float targetFieldOfViewDegrees = GetTargetFieldOfViewDegrees(baseFieldOfViewDegrees);
        float fieldOfViewFactor = Lerp(1, targetFieldOfViewDegrees / baseFieldOfViewDegrees, _zoomProgress);

        ZoomRuntimeState.FieldOfViewFactor = fieldOfViewFactor;
        ZoomRuntimeState.MouseSensitivityFactor = Lerp(1, fieldOfViewFactor, GetMouseSensitivityReductionStrength());
        _clientMain.MainCamera.Fov = baseFieldOfViewDegrees * DegreesToRadians;
        _clientMain.Reset3DProjection();
        _overlayRenderer?.SetZoom(_zoomProgress, GetVignetteStrength());
    }

    private float GetTargetFieldOfViewDegrees(float baseFieldOfViewDegrees)
    {
        return Math.Max(MinimumFieldOfViewDegrees, baseFieldOfViewDegrees / (Config.Strength + 1f));
    }

    private float GetZoomSpeed()
    {
        return 1 + Math.Clamp(Config.Speed, 1, 10) * 0.9f;
    }

    private float GetMouseSensitivityReductionStrength()
    {
        float normalizedZoomStrength = (Math.Clamp(Config.Strength, 1, 10) - 1) / 9f;

        return Lerp(MinimumMouseSensitivityReductionStrength, MaximumMouseSensitivityReductionStrength, normalizedZoomStrength);
    }

    private float GetVignetteStrength()
    {
        if (Config.VignetteStrength <= 0)
        {
            return 0;
        }

        float clampedStrength = Math.Clamp(Config.VignetteStrength, 1, 10);

        return 0.25f + clampedStrength * 0.15f + VignetteStrengthBoost;
    }

    private static float MoveTowards(float currentValue, float targetValue, float maxDelta)
    {
        if (Math.Abs(targetValue - currentValue) <= maxDelta)
        {
            return targetValue;
        }

        return currentValue + Math.Sign(targetValue - currentValue) * maxDelta;
    }

    private static float Lerp(float startValue, float endValue, float amount)
    {
        return startValue + (endValue - startValue) * amount;
    }
}
