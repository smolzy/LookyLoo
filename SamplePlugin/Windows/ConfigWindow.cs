using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Numerics;

namespace SamplePlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private readonly Plugin plugin;

    public ConfigWindow(Plugin plugin)
        : base("LookyLoo - Configuration###LookyLooConfigWindow")
    {
        Flags = ImGuiWindowFlags.NoCollapse;
        Size = new Vector2(360, 480);
        SizeCondition = ImGuiCond.FirstUseEver;

        this.plugin = plugin;
        this.configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        if (configuration.IsConfigWindowMovable)
            Flags &= ~ImGuiWindowFlags.NoMove;
        else
            Flags |= ImGuiWindowFlags.NoMove;
    }

    public override void Draw()
    {
        // === Detection ===
        ImGui.TextColored(configuration.TitleColor, "Detection");
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Text("Maximum detection distance:");
        var maxDist = configuration.MaxDistance;
        if (ImGui.SliderFloat("##MaxDistance", ref maxDist, 5.0f, 200.0f, "%.0f m"))
        {
            configuration.MaxDistance = maxDist;
            configuration.Save();
        }

        ImGui.Spacing();
        ImGui.Spacing();

        // === Behaviour ===
        ImGui.TextColored(configuration.TitleColor, "Behavior");
        ImGui.Separator();
        ImGui.Spacing();

        var autoTarget = configuration.AutoTargetOnLeftClick;
        if (ImGui.Checkbox("Target on left click", ref autoTarget))
        {
            configuration.AutoTargetOnLeftClick = autoTarget;
            configuration.Save();
        }

        var highlight = configuration.HighlightOnHover;
        if (ImGui.Checkbox("Auto-focus on hover", ref highlight))
        {
            configuration.HighlightOnHover = highlight;
            configuration.Save();
        }

        ImGui.Spacing();
        ImGui.Spacing();

        // === Context Menu Options ===
        ImGui.TextColored(configuration.TitleColor, "Context Menu Options");
        ImGui.Separator();
        ImGui.Spacing();

        var showTarget = configuration.ShowTargetOption;
        if (ImGui.Checkbox("Target Player", ref showTarget)) { configuration.ShowTargetOption = showTarget; configuration.Save(); }

        var showFocus = configuration.ShowFocusTargetOption;
        if (ImGui.Checkbox("Focus Target", ref showFocus)) { configuration.ShowFocusTargetOption = showFocus; configuration.Save(); }

        var showNative = configuration.ShowNativeMenuOption;
        if (ImGui.Checkbox("Open Native Subcommand", ref showNative)) { configuration.ShowNativeMenuOption = showNative; configuration.Save(); }

        var showExamine = configuration.ShowExamineOption;
        if (ImGui.Checkbox("Examine", ref showExamine)) { configuration.ShowExamineOption = showExamine; configuration.Save(); }

        var showAdventurePlate = configuration.ShowAdventurePlateOption;
        if (ImGui.Checkbox("View Adventurer Plate", ref showAdventurePlate)) { configuration.ShowAdventurePlateOption = showAdventurePlate; configuration.Save(); }

        var showCopy = configuration.ShowCopyNameOption;
        if (ImGui.Checkbox("Copy Name", ref showCopy)) { configuration.ShowCopyNameOption = showCopy; configuration.Save(); }

        ImGui.Spacing();
        ImGui.Spacing();

        // === Colors ===
        ImGui.TextColored(configuration.TitleColor, "Colors");
        ImGui.Separator();
        ImGui.Spacing();

        var titleColor = configuration.TitleColor;
        if (ImGui.ColorEdit4("Title Color##titleColor", ref titleColor)) { configuration.TitleColor = titleColor; configuration.Save(); }

        var targetingColor = configuration.TargetingMeColor;
        if (ImGui.ColorEdit4("Targeting Me##targetingColor", ref targetingColor)) { configuration.TargetingMeColor = targetingColor; configuration.Save(); }

        var nearbyColor = configuration.NearbyPlayerColor;
        if (ImGui.ColorEdit4("Nearby Player##nearbyColor", ref nearbyColor)) { configuration.NearbyPlayerColor = nearbyColor; configuration.Save(); }

        var unloadedColor = configuration.UnloadedColor;
        if (ImGui.ColorEdit4("Out of Range##unloadedColor", ref unloadedColor)) { configuration.UnloadedColor = unloadedColor; configuration.Save(); }
    }
}
