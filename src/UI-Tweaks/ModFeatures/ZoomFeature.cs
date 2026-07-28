using BitzArt.UI.Tweaks.Config;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace BitzArt.UI.Tweaks;

public sealed class ZoomFeature(UiTweaksModSystem modSystem, ZoomConfig config)
    : ModSystemFeature<UiTweaksModSystem, ZoomConfig>(modSystem, config), IRenderer
{
    private ICoreClientAPI? _clientApi;
    private ClientMain? _clientMain;
    private Harmony? _harmony;
    private MeshRef? _quadMesh;
    private IShaderProgram? _shaderProgram;
    private bool _isRendererRegistered;
    private bool _isRendererUnregistrationQueued;
    private bool _isZoomRequested;
    private float _zoomProgress;
    private float _vignetteStrength;

    public double RenderOrder => 0.05;

    public int RenderRange => 1;

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client && Config.Enable;

    public override void Start(ICoreClientAPI clientApi)
    {
        if (clientApi.World is not ClientMain clientMain)
        {
            throw new InvalidOperationException("Unable to retrieve ClientMain from ICoreClientAPI.");
        }

        _clientApi = clientApi;
        _clientMain = clientMain;
        _harmony = new Harmony($"{Constants.ModId}.zoom");

        var quadMeshData = QuadMeshUtil.GetCustomQuadModelData(-1, -1, 0, 2, 2);
        quadMeshData.Rgba = null;
        _quadMesh = clientApi.Render.UploadMesh(quadMeshData);

        LoadShader();
        clientApi.Event.ReloadShader += LoadShader;

        ZoomProjectionPatch.Patch(_harmony);
        ZoomMouseSensitivityPatch.Patch(_harmony);

        clientApi.Input.AddHotKey(ModHotKeys.Zoom, mapping =>
        {
            _isZoomRequested = !mapping.OnKeyUp;

            if (!_isRendererRegistered)
            {
                _clientApi!.Event.RegisterRenderer(this, EnumRenderStage.Done, $"{Constants.ModId}.zoom");
                _isRendererRegistered = true;
            }

            return true;
        });

        clientApi.Input.GetHotKeyByCode(ModHotKeys.Zoom.Code).TriggerOnUpAlso = true;
    }

    public override void Dispose()
    {
        UnregisterRenderer();

        _isRendererUnregistrationQueued = false;
        _isZoomRequested = false;
        _zoomProgress = 0;

        if (_clientApi is not null)
        {
            _clientApi.Input.SetHotKeyHandler(ModHotKeys.Zoom.Code, null);
            _clientApi.Event.ReloadShader -= LoadShader;

            ApplyZoomProgress();

            _shaderProgram?.Dispose();
            _shaderProgram = null;

            if (_quadMesh is not null)
            {
                _clientApi.Render.DeleteMesh(_quadMesh);
                _quadMesh = null;
            }
        }

        _harmony?.UnpatchAll(_harmony.Id);
        _harmony = null;

        _clientApi = null;
        _clientMain = null;

        GC.SuppressFinalize(this);
    }

    public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
    {
        RenderOverlay();
        UpdateZoom(deltaTime);
    }

    private void UnregisterRenderer()
    {
        if (!_isRendererRegistered)
        {
            return;
        }

        _clientApi!.Event.UnregisterRenderer(this, EnumRenderStage.Done);
        _isRendererRegistered = false;
    }

    private bool IsZoomStable() => !_isZoomRequested && _zoomProgress == 0;

    private void RenderOverlay()
    {
        if (!_clientMain!.ShouldRender2DOverlays
            || _quadMesh is null
            || _shaderProgram is null
            || _zoomProgress <= 0
            || _vignetteStrength <= 0)
        {
            return;
        }

        var renderApi = _clientApi!.Render;
        var previousShaderProgram = renderApi.CurrentActiveShader;
        previousShaderProgram?.Stop();

        _shaderProgram.Use();
        renderApi.GlToggleBlend(true, EnumBlendMode.Standard);
        _shaderProgram.Uniform("zoomProgress", _zoomProgress);
        _shaderProgram.Uniform("vignetteStrength", _vignetteStrength);
        renderApi.RenderMesh(_quadMesh);
        _shaderProgram.Stop();

        previousShaderProgram?.Use();
    }

    private void UpdateZoom(float deltaTime)
    {
        const float returnZoomSpeedMultiplier = 2;

        var zoomSpeed = 1 + Math.Clamp(Config.Speed, 1, 10) * 0.9f;
        var zoomDirection = _isZoomRequested ? 1 : -returnZoomSpeedMultiplier;
        var zoomProgress = Math.Clamp(
            _zoomProgress + zoomDirection * zoomSpeed * deltaTime,
            0,
            1);

        if (zoomProgress != _zoomProgress)
        {
            _zoomProgress = zoomProgress;
            ApplyZoomProgress();
        }

        if (!_isRendererRegistered
            || _isRendererUnregistrationQueued
            || !IsZoomStable())
        {
            return;
        }

        _isRendererUnregistrationQueued = true;
        _clientApi!.Event.EnqueueMainThreadTask(() =>
        {
            _isRendererUnregistrationQueued = false;

            if (!IsZoomStable())
            {
                return;
            }

            UnregisterRenderer();
        }, $"{Constants.ModId}.zoom.unregister");
    }

    private void ApplyZoomProgress()
    {
        const float minimumFieldOfViewDegrees = 10;
        const float minimumMouseSensitivityReductionStrength = 0.35f;
        const float maximumMouseSensitivityReductionStrength = 1;

        var baseFieldOfViewDegrees = Math.Max(minimumFieldOfViewDegrees, _clientApi!.Settings.Int["fieldOfView"]);
        var zoomStrength = Math.Clamp(Config.Strength, 1, 10);
        var targetFieldOfViewDegrees = Math.Max(
            minimumFieldOfViewDegrees,
            baseFieldOfViewDegrees / (zoomStrength + 1f));

        var fieldOfViewFactor = float.Lerp(1, targetFieldOfViewDegrees / baseFieldOfViewDegrees, _zoomProgress);
        var mouseSensitivityReductionStrength = float.Lerp(
            minimumMouseSensitivityReductionStrength,
            maximumMouseSensitivityReductionStrength,
            (zoomStrength - 1) / 9f);
        _vignetteStrength = Config.VignetteStrength <= 0
            ? 0
            : 0.25f + Math.Clamp(Config.VignetteStrength, 1, 10) * 0.15f;

        ZoomRuntimeState.FieldOfViewFactor = fieldOfViewFactor;
        ZoomRuntimeState.MouseSensitivityFactor = float.Lerp(1, fieldOfViewFactor, mouseSensitivityReductionStrength);
        _clientMain!.MainCamera.Fov = float.DegreesToRadians(baseFieldOfViewDegrees);
        _clientMain.Reset3DProjection();
    }

    private bool LoadShader()
    {
        _shaderProgram?.Dispose();

        var shaderApi = _clientApi!.Shader;
        var shaderProgram = shaderApi.NewShaderProgram();
        shaderProgram.VertexShader = shaderApi.NewShader(EnumShaderType.VertexShader);
        shaderProgram.FragmentShader = shaderApi.NewShader(EnumShaderType.FragmentShader);
        shaderProgram.VertexShader.Code = GetVertexShaderCode();
        shaderProgram.FragmentShader.Code = GetFragmentShaderCode();

        shaderApi.RegisterMemoryShaderProgram("uitweakszoomoverlay", shaderProgram);

        _shaderProgram = shaderProgram;

        return shaderProgram.Compile();
    }

    private static string GetVertexShaderCode() =>
        """
        #version 330 core
        #extension GL_ARB_explicit_attrib_location: enable

        layout(location = 0) in vec3 vertex;

        out vec2 uv;

        void main(void) {
            gl_Position = vec4(vertex.xy, 0, 1);
            uv = (vertex.xy + 1.0) / 2.0;
        }
        """;

    private static string GetFragmentShaderCode() =>
        """
        #version 330 core

        in vec2 uv;
        out vec4 outColor;

        uniform float zoomProgress;
        uniform float vignetteStrength;

        void main(void) {
            float strength = max(vignetteStrength, 0.0);
            float rangeStrength = clamp(strength, 0.0, 1.0);
            float extraRangeStrength = clamp(strength - 1.0, 0.0, 1.0);
            float distanceFromCenter = distance(uv.xy, vec2(0.5, 0.5));
            float vignetteStart = mix(0.34, 0.12, rangeStrength) - extraRangeStrength * 0.05;
            float vignetteEnd = mix(0.78, 0.58, rangeStrength) - extraRangeStrength * 0.10;
            float edgeDarkness = smoothstep(vignetteStart, vignetteEnd, distanceFromCenter);
            float alphaStrength = min(strength, 1.0);
            float alpha = edgeDarkness * alphaStrength * clamp(zoomProgress, 0.0, 1.0);
            outColor = vec4(0.0, 0.0, 0.0, alpha);
        }
        """;
}
