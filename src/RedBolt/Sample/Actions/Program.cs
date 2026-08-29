// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Sample.Actions;

using System;
using System.Collections.Generic;

using RedBolt;
using RedBolt.Actions;
using RedBolt.Bindings.DearImGui;
using RedBolt.Container;
using RedBolt.Core;
using RedBolt.DearImGui;
using RedBolt.Engine;
using RedBolt.Graphics;
using RedBolt.Input;
using RedBolt.Math;
using RedBolt.Scene;

internal static class ActionsProgram
{
    private static int Main()
    {
        try
        {
            return new ActionsApplication().Run();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }
}

sealed class ActionsApplication : Application
{
    private struct ActionPulseEvent
    {
        public int Count;
    }

    private sealed class ActionDemo : IDisposable
    {
        public ActionDemo(string name, FiniteTimeAction action, bool targetsMaterial = false, string? status = null)
        {
            Name = name;
            Action = action;
            TargetsMaterial = targetsMaterial;
            Status = status ?? $"Previewing: {name}";
            Label = new CString(name);
        }

        public void Dispose()
        {
            Label.Dispose();
            Action.Dispose();
        }

        public string Name { get; }
        public FiniteTimeAction Action { get; }
        public bool TargetsMaterial { get; }
        public string Status { get; }
        public CString Label { get; }
    }

    private static readonly Vector3f BallPosition = new(0.0f, 1.25f, 0.0f);
    private static readonly Vector4f BallColor = new(0.1f, 0.75f, 1.0f, 1.0f);
    private readonly List<ActionDemo> _actions = [];
    private readonly CString _status = new("Select an action to preview it");
    private readonly CString _running = new();
    private static readonly Vector4f WindowBackground = new(0.025f, 0.035f, 0.075f, 0.96f);
    private static readonly Vector4f PanelBackground = new(0.045f, 0.06f, 0.105f, 0.96f);
    private static readonly Vector4f PanelBorder = new(0.12f, 0.28f, 0.42f, 0.85f);
    private static readonly Vector4f ControlBackground = new(0.075f, 0.11f, 0.17f, 1.0f);
    private static readonly Vector4f ControlHovered = new(0.08f, 0.32f, 0.44f, 1.0f);
    private static readonly Vector4f ControlActive = new(0.1f, 0.48f, 0.58f, 1.0f);
    private static readonly Vector4f AccentCyan = new(0.25f, 0.9f, 1.0f, 1.0f);
    private static readonly Vector4f MutedText = new(0.62f, 0.7f, 0.8f, 1.0f);
    private Scene? _scene;
    private Node? _cameraNode;
    private Viewport? _viewport;
    private ActionManager? _actionManager;
    private Node? _ballNode;
    private Material? _ballMaterial;
    private int _callbackCount;
    private int _eventCount;

    protected override void Setup(EngineParameters parameters)
    {
        parameters.ApplicationName = "RedBolt.Sample.Actions";
        parameters.Borderless = false;
        parameters.FullScreen = false;
        parameters.ResourcePaths = "CoreData;SampleData";
        parameters.ResourcePrefixPaths = ".;..";
        parameters.WindowMaximize = false;
#if !REDBOLT_PLATFORM_BROWSER
            parameters.WindowHeight = 720;
            parameters.WindowWidth = 1280;
#endif
        parameters.WindowTitle = "RedBolt Actions";
    }

    protected override void Start()
    {
        _actionManager = new ActionManager();
        Context.RegisterSubsystem(_actionManager);
        Context.RegisterSubsystem(new DearImGui((ImGuiConfigFlags)Engine.Parameters.DearImGuiFlags));
        SubscribeToEvent<DearImGuiEvents.EndRenderingDearImGuiEvent>(
            (ref DearImGuiEvents.EndRenderingDearImGuiEvent _) => DrawControls());

        CreateScene();
        CreateBall();
        CreateActions();
        SetupViewport();

        Input.SetMouseMode(MouseMode.Free);
        Input.SetMouseVisible(true);
    }

    private void CreateScene()
    {
        _scene = new Scene();
        _scene.CreateComponent<Octree>();

        var zone = _scene.CreateChild("Zone").CreateComponent<Zone>();
        zone.SetBoundingBox(new BoundingBox(-100.0f, 100.0f));
        zone.SetAmbientColor(new Color(0.025f, 0.035f, 0.08f));
        zone.SetFogColor(new Color(0.005f, 0.01f, 0.035f));
        zone.SetFogStart(30.0f);
        zone.SetFogEnd(55.0f);

        var floorNode = _scene.CreateChild("Floor");
        floorNode.SetScale(new Vector3f(38.0f, 1.0f, 38.0f));
        var floor = floorNode.CreateComponent<StaticModel>();
        floor.SetModel(Resources.Get<Model>("Models/Plane.mdl"));
        floor.SetMaterial(Resources.Get<Material>("Materials/TechnicalDemo/SquareFloorGrid.xml"));

        var directionalLightNode = _scene.CreateChild("DirectionalLight");
        directionalLightNode.SetDirection(new Vector3f(0.4f, -1.0f, 0.25f));
        var directionalLight = directionalLightNode.CreateComponent<Light>();
        directionalLight.SetLightType(LightType.Directional);
        directionalLight.SetBrightness(0.8f);
        directionalLight.SetColor(new Color(0.35f, 0.42f, 0.65f));

        CreatePointLight(new Vector3f(-9.0f, 6.0f, -6.0f), new Color(0.1f, 0.45f, 1.0f));
        CreatePointLight(new Vector3f(9.0f, 6.0f, -2.0f), new Color(1.0f, 0.15f, 0.45f));
        CreatePointLight(new Vector3f(0.0f, 7.0f, 9.0f), new Color(0.3f, 1.0f, 0.55f));

        _cameraNode = _scene.CreateChild("Camera");
        _cameraNode.CreateComponent<Camera>().SetFarClip(100.0f);
        // Leave the subject unobstructed beside the fixed control panel.
        _cameraNode.SetPosition(new Vector3f(-4.5f, 19.0f, -28.0f));
        _cameraNode.LookAt(new Vector3f(-4.5f, 1.5f, 0.0f));
    }

    private void CreatePointLight(in Vector3f position, in Color color)
    {
        var node = _scene!.CreateChild("PointLight");
        node.SetPosition(position);
        var light = node.CreateComponent<Light>();
        light.SetLightType(LightType.Point);
        light.SetRange(22.0f);
        light.SetBrightness(2.3f);
        light.SetColor(color);
    }

    private void CreateActions()
    {
        var offset = new Vector3f(0.0f, 4.2f, 0.0f);

        AddAction("Linear", new ActionBuilder().MoveBy(2.8f, offset).DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Back In", new ActionBuilder().MoveBy(2.8f, offset).BackIn().DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Back Out", new ActionBuilder().MoveBy(2.8f, offset).BackOut().DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Back In/Out", new ActionBuilder().MoveBy(2.8f, offset).BackInOut().DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Bounce Out", new ActionBuilder().MoveBy(2.8f, offset).BounceOut().DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Bounce In", new ActionBuilder().MoveBy(2.8f, offset).BounceIn().DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Bounce In/Out", new ActionBuilder().MoveBy(2.8f, offset).BounceInOut().DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Sine Out", new ActionBuilder().MoveBy(2.8f, offset).SineOut().DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Sine In", new ActionBuilder().MoveBy(2.8f, offset).SineIn().DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Sine In/Out", new ActionBuilder().MoveBy(2.8f, offset).SineInOut().DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Exponential Out", new ActionBuilder().MoveBy(2.8f, offset).ExponentialOut().DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Exponential In", new ActionBuilder().MoveBy(2.8f, offset).ExponentialIn().DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Exponential In/Out", new ActionBuilder().MoveBy(2.8f, offset).ExponentialInOut().DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Elastic In", new ActionBuilder().MoveBy(2.8f, offset).ElasticIn().DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Elastic Out", new ActionBuilder().MoveBy(2.8f, offset).ElasticOut().DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Elastic In/Out", new ActionBuilder().MoveBy(2.8f, offset).ElasticInOut().DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Quadratic", new ActionBuilder()
            .MoveByQuadratic(3.0f, offset * 0.5f + new Vector3f(1.5f, 0.0f, 0.0f), offset)
            .DelayTime(0.4f).JumpBy(-offset).RepeatForever().Build()!);

        var spin = new ActionBuilder().RotateBy(2.0f, new Quaternion(0.0f, 180.0f, 0.0f)).Build()!;
        AddAction("Parallel", new ActionBuilder().MoveBy(2.0f, offset).Also(spin).DelayTime(0.5f)
            .JumpBy(-offset).RepeatForever().Build()!);
        AddAction("Scale + Repeat", new ActionBuilder()
            .ScaleBy(1.5f, new Vector3f(1.35f, 1.35f, 1.35f)).SineInOut().Repeat(2).Build()!);

        AddAction("Rotate Around", new ActionBuilder()
            .RotateAround(3.0f, BallPosition + new Vector3f(0.0f, 0.0f, 2.8f),
                new Quaternion(0.0f, 180.0f, 0.0f))
            .RepeatForever().Build()!);
        AddAction("Blink", new ActionBuilder().Blink(2.5f, 8).DelayTime(0.4f).RepeatForever().Build()!);
        AddAction("Enable / Disable", new ActionBuilder()
            .Disable().DelayTime(0.35f).Enable().DelayTime(1.2f).RepeatForever().Build()!);

        using (var from = new Variant(BallPosition))
        using (var to = new Variant(BallPosition + offset))
        {
            AddAction("Attribute From/To", new ActionBuilder()
                .AttributeFromTo(4.0f, AttributeActionConstants.PositionAttribute, from, to).Build()!);
        }

        var shaderAccent = new Vector4f(1.0f, 0.12f, 0.02f, 1.0f);
        using (var from = new Variant(BallColor))
        using (var to = new Variant(shaderAccent))
        {
            AddAction("Shader Parameter", new ActionBuilder()
                    .ShaderParameterFromTo(1.2f, "MatDiffColor", from, to).SineInOut()
                    .ShaderParameterFromTo(1.2f, "MatDiffColor", to, from).SineInOut()
                    .RepeatForever().Build()!,
                targetsMaterial: true, status: "Animating MatDiffColor: cyan to orange");
        }

        AddAction("Callback", new ActionBuilder().DelayTime(1.0f)
            .CallFunc(this, static (application, _) => application.HandleCallback()).DelayTime(2.0f)
            .RepeatForever().Build()!);
        AddAction("Typed Event", new ActionBuilder().DelayTime(1.5f)
            .SendEvent(new ActionPulseEvent { Count = 1 }).DelayTime(1.5f).RepeatForever().Build()!);
        SubscribeToEvent<ActionPulseEvent>(_ballNode!, HandleActionPulse);
        AddAction("Remove Self", new ActionBuilder()
                .ScaleBy(1.4f, new Vector3f(0.04f, 0.04f, 0.04f)).SineInOut()
                .RemoveSelf().Build()!,
            status: "Shrinking, then removing the ball from the scene");
    }

    private void CreateBall()
    {
        _ballMaterial = Resources.Get<Material>("Materials/Constant/GlossyWhite.xml").Clone();
        _ballMaterial.SetShaderParameter("MatDiffColor", BallColor);
        _ballMaterial.SetShaderParameter("MatEmissiveColor",
            new Vector3f(BallColor.X * 0.22f, BallColor.Y * 0.22f, BallColor.Z * 0.22f));

        var pedestalNode = _scene!.CreateChild("ActionPedestal");
        pedestalNode.SetPosition(new Vector3f(BallPosition.X, 0.18f, BallPosition.Z));
        pedestalNode.SetScale(new Vector3f(3.2f, 0.18f, 3.2f));
        var pedestal = pedestalNode.CreateComponent<StaticModel>();
        pedestal.SetModel(Resources.Get<Model>("Models/Cylinder.mdl"));
        pedestal.SetMaterial(Resources.Get<Material>("Materials/Constant/DielectricG3.xml"));

        _ballNode = _scene.CreateChild("ActionBall");
        _ballNode.SetPosition(BallPosition);
        _ballNode.SetScale(2.0f);
        var model = _ballNode.CreateComponent<StaticModel>();
        model.SetModel(Resources.Get<Model>("Models/Sphere.mdl"));
        model.SetMaterial(_ballMaterial);
        model.SetCastShadows(true);
    }

    private void AddAction(string name, FiniteTimeAction action, bool targetsMaterial = false, string? status = null)
    {
        _actions.Add(new ActionDemo(name, action, targetsMaterial, status));
    }

    private void SetupViewport()
    {
        _viewport = new Viewport(_scene!, _cameraNode!.GetComponent<Camera>()!);
        Renderer.SetViewport(0, _viewport);
    }

    private void ResetBall()
    {
        _actionManager!.CancelAllActions();
        if (_ballNode!.Parent is null)
            _scene!.AddChild(_ballNode);
        _ballNode.SetEnabled(true);
        _ballNode.SetPosition(BallPosition);
        _ballNode.SetRotation(Quaternion.IDENTITY);
        _ballNode.SetScale(2.0f);
        _ballMaterial!.SetShaderParameter("MatDiffColor", BallColor);
    }

    private void Run(ActionDemo action)
    {
        ResetBall();
        _actionManager!.AddAction(action.Action, action.TargetsMaterial ? _ballMaterial! : _ballNode!);
        _status.FormatInPlace(action.Status);
    }

    private void HandleCallback()
    {
        ++_callbackCount;
        _status.FormatInPlace($"Callback fired {_callbackCount} times");
    }

    private void HandleActionPulse(ref ActionPulseEvent eventData)
    {
        _eventCount += eventData.Count;
        _status.FormatInPlace($"Typed event received {_eventCount} times");
    }

    private void DrawControls()
    {
        var activeActions = _actionManager!.GetNumActions(_ballNode!) +
            _actionManager.GetNumActions(_ballMaterial!);
        _running.FormatInPlace(activeActions == 0 ? "Ball ready" : "Action running");

        ImGui.SetNextWindowPos(new Vector2f(12.0f, 12.0f), ImGuiCond.ImGuiCondAlways);
        ImGui.SetNextWindowSize(new Vector2f(380.0f, 696.0f), ImGuiCond.ImGuiCondAlways);
        ImGui.PushStyleVar(ImGuiStyleVar.ImGuiStyleVarWindowRounding, 10.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ImGuiStyleVarFrameRounding, 5.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ImGuiStyleVarWindowPadding, new Vector2f(14.0f, 12.0f));
        ImGui.PushStyleColor(ImGuiCol.ImGuiColWindowBg, WindowBackground);
        ImGui.PushStyleColor(ImGuiCol.ImGuiColBorder, PanelBorder);
        ImGui.PushStyleColor(ImGuiCol.ImGuiColButton, ControlBackground);
        ImGui.PushStyleColor(ImGuiCol.ImGuiColButtonHovered, ControlHovered);
        ImGui.PushStyleColor(ImGuiCol.ImGuiColButtonActive, ControlActive);
        var windowOpen = true;
        if (ImGui.Begin("Actions"u8, ref windowOpen,
            ImGuiWindowFlags.ImGuiWindowFlagsNoMove |
            ImGuiWindowFlags.ImGuiWindowFlagsNoResize |
            ImGuiWindowFlags.ImGuiWindowFlagsNoSavedSettings))
        {
            ImGui.PushStyleColor(ImGuiCol.ImGuiColText, AccentCyan);
            ImGui.TextUnformatted("INTERACTIVE ACTION PREVIEW"u8);
            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.ImGuiColText, MutedText);
            ImGui.TextUnformatted("Select an action and watch it run on the ball.\nChoosing another action resets the preview."u8);
            ImGui.PopStyleColor();

            ImGui.Dummy(new Vector2f(1.0f, 4.0f));
            if (ImGui.Button("RESET BALL"u8, new Vector2f(160.0f, 30.0f)))
            {
                ResetBall();
                _status.FormatInPlace("Select an action to preview it");
            }

            ImGui.PushStyleColor(ImGuiCol.ImGuiColChildBg, PanelBackground);
            ImGui.PushStyleColor(ImGuiCol.ImGuiColBorder, PanelBorder);
            ImGui.PushStyleVar(ImGuiStyleVar.ImGuiStyleVarChildRounding, 7.0f);
            if (ImGui.BeginChild("##Status"u8, new Vector2f(0.0f, 61.0f), ImGuiChildFlags.ImGuiChildFlagsBorders))
            {
                ImGui.PushStyleColor(ImGuiCol.ImGuiColText, AccentCyan);
                ImGui.TextUnformatted(_running.ContentBytes);
                ImGui.PopStyleColor();
                ImGui.TextUnformatted(_status.ContentBytes);
            }
            ImGui.EndChild();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor(2);

            ImGui.PushStyleColor(ImGuiCol.ImGuiColText, MutedText);
            ImGui.TextUnformatted("CHOOSE AN EFFECT"u8);
            ImGui.PopStyleColor();

            ImGui.PushStyleColor(ImGuiCol.ImGuiColChildBg, PanelBackground);
            ImGui.PushStyleColor(ImGuiCol.ImGuiColBorder, PanelBorder);
            ImGui.PushStyleVar(ImGuiStyleVar.ImGuiStyleVarChildRounding, 7.0f);
            if (ImGui.BeginChild("##Tracks"u8, new Vector2f(0.0f, 0.0f), ImGuiChildFlags.ImGuiChildFlagsBorders))
            {
                for (var i = 0; i < _actions.Count; ++i)
                {
                    if (ImGui.Button(_actions[i].Label.ContentBytes, new Vector2f(160.0f, 0.0f)))
                        Run(_actions[i]);
                    if (i % 2 == 0)
                        ImGui.SameLine();
                }
            }
            ImGui.EndChild();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor(2);
        }
        ImGui.End();
        ImGui.PopStyleColor(5);
        ImGui.PopStyleVar(3);
    }

    protected override void Update(float timeStep)
    {
        if (Input.ExitRequested || Input.GetKeyPress(Key.Escape))
            Exit();
    }

    protected override void Stop()
    {
        DisposeScene();
        base.Stop();
    }

    public override void Dispose()
    {
        DisposeScene();
        _running.Dispose();
        _status.Dispose();
        base.Dispose();
    }

    private void DisposeScene()
    {
        if (_scene is null)
            return;

        _actionManager?.CancelAllActions();

        Renderer.SetNumViewports(0);
        _viewport?.Dispose();
        _viewport = null;
        _cameraNode = null;
        _ballNode = null;
        _scene.Dispose();
        _scene = null;
        _ballMaterial?.Dispose();
        _ballMaterial = null;

        for (var i = _actions.Count - 1; i >= 0; --i)
            _actions[i].Dispose();
        _actions.Clear();
    }
}
